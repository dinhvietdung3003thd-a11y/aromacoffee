using Dapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DTOs.order;
using WebApplication1.Hubs;
using WebApplication1.Models;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class OrderService : IOrderService
    {
        private readonly IDbConnection _db;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderService(IDbConnection db, IHubContext<OrderHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        // =========================
        // GET ALL
        // =========================
        public async Task<IEnumerable<OrderDisplayDTO>> GetAllAsync()
        {
            const string sql = @" SELECT o.order_id AS Id,
                                         o.created_at AS OrderDate,
                                         o.total_amount AS TotalAmount,
                                         o.status AS Status,
                                         o.table_id AS TableId,
                                         o.user_id AS UserId,
                                         o.customer_id AS CustomerId,
                                         o.note AS Note,
                                         u.full_name AS CreatorFullName
                                   FROM orders o
                                   LEFT JOIN users u ON o.user_id = u.user_id
                                   ORDER BY o.created_at DESC;";

            return await _db.QueryAsync<OrderDisplayDTO>(sql);
        }

        // =========================
        // GET BY ID (kèm details)
        // =========================
        public async Task<OrderDisplayDTO?> GetByIdAsync(int id)
        {
            const string orderSql = @" SELECT o.order_id AS Id,
                                              o.created_at AS OrderDate,
                                              o.total_amount AS TotalAmount,
                                              o.status AS Status,
                                              o.table_id AS TableId,
                                              o.user_id AS UserId,
                                              o.customer_id AS CustomerId,
                                              o.note AS Note,
                                              u.full_name AS CreatorFullName
                                       FROM orders o
                                       LEFT JOIN users u ON o.user_id = u.user_id
                                       WHERE o.order_id = @Id;";

            var order = await _db.QueryFirstOrDefaultAsync<OrderDisplayDTO>(orderSql, new { Id = id });
            
            if (order == null) return null;

            const string detailSql = @" SELECT od.order_detail_id AS Id,
                                               od.order_id AS OrderId,
                                               od.product_id AS ProductId,
                                               p.name AS ProductName,
                                               od.quantity AS Quantity,
                                               od.unit_price AS UnitPrice,
                                               od.subtotal AS Subtotal
                                       FROM order_details od
                                       LEFT JOIN products p ON od.product_id = p.product_id
                                       WHERE od.order_id = @Id
                                       ORDER BY od.order_detail_id ASC;";

            var details = await _db.QueryAsync<OrderDetailDTO>(detailSql, new { Id = id });

            order.Details = details.ToList();

            return order;
        }

        // =========================
        // ADD (DINE-IN ONLY) - KHÔNG SHIP
        // Server tự tính total từ DB price
        // =========================
        private async Task<(int OrderId, decimal TotalAmount)> AddInternalAsync(
    DateTime orderDate,
    int? tableId,
    string? status,
    int? userId,
    int? customerId,
    string? note,
    List<OrderDetailCreateDTO> details)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            if (details == null || details.Count == 0)
                throw new ArgumentException("Order phải có ít nhất 1 sản phẩm.");

            if (details.Any(d => d.Quantity <= 0))
                throw new ArgumentException("Quantity phải >= 1.");

            using var transaction = _db.BeginTransaction();

            try
            {
                // 1. Nếu có bàn thì kiểm tra bàn tồn tại và còn trống không
                if (tableId.HasValue)
                {
                    const string checkTableSql = @"
                SELECT status
                FROM tables
                WHERE table_id = @TableId;
            ";

                    var tableStatus = await _db.QueryFirstOrDefaultAsync<string>(
                        checkTableSql,
                        new { TableId = tableId.Value },
                        transaction
                    );

                    if (tableStatus == null)
                        throw new Exception("Bàn không tồn tại.");

                    if (tableStatus != "Available")
                        throw new Exception("Bàn đang được sử dụng.");
                }

                // 2. Insert order trước
                const string insertOrderSql = @"
            INSERT INTO orders
            (created_at, total_amount, user_id, table_id, status, customer_id, note)
            VALUES
            (@OrderDate, 0, @UserId, @TableId, @Status, @CustomerId, @Note);

            SELECT LAST_INSERT_ID();
        ";

                int orderId = await _db.ExecuteScalarAsync<int>(
                    insertOrderSql,
                    new
                    {
                        OrderDate = orderDate,
                        UserId = userId,
                        TableId = tableId,
                        Status = string.IsNullOrWhiteSpace(status) ? "Pending" : status,
                        CustomerId = customerId,
                        Note = note
                    },
                    transaction
                );

                // 3. Insert order details + tính total
                const string productPriceSql = @"
            SELECT price
            FROM products
            WHERE product_id = @ProductId AND is_available = 1;
        ";

                const string insertDetailSql = @"
            INSERT INTO order_details
            (order_id, product_id, quantity, unit_price, subtotal)
            VALUES
            (@OrderId, @ProductId, @Quantity, @UnitPrice, @Subtotal);
        ";

                decimal totalAmount = 0;

                foreach (var d in details)
                {
                    var unitPrice = await _db.QueryFirstOrDefaultAsync<decimal?>(
                        productPriceSql,
                        new { ProductId = d.ProductId },
                        transaction
                    );

                    if (unitPrice == null)
                        throw new Exception($"Sản phẩm có ID = {d.ProductId} không tồn tại hoặc không khả dụng.");

                    decimal subtotal = unitPrice.Value * d.Quantity;
                    totalAmount += subtotal;

                    await _db.ExecuteAsync(
                        insertDetailSql,
                        new
                        {
                            OrderId = orderId,
                            ProductId = d.ProductId,
                            Quantity = d.Quantity,
                            UnitPrice = unitPrice.Value,
                            Subtotal = subtotal
                        },
                        transaction
                    );
                }

                // 4. Update total_amount sau khi tính xong
                const string updateTotalSql = @"
            UPDATE orders
            SET total_amount = @TotalAmount
            WHERE order_id = @OrderId;
        ";

                await _db.ExecuteAsync(
                    updateTotalSql,
                    new
                    {
                        OrderId = orderId,
                        TotalAmount = totalAmount
                    },
                    transaction
                );

                // 5. Nếu có bàn thì chuyển sang Occupied
                if (tableId.HasValue)
                {
                    const string occupyTableSql = @"
                UPDATE tables
                SET status = 'Occupied'
                WHERE table_id = @TableId
                  AND status = 'Available';
            ";

                    int affected = await _db.ExecuteAsync(
                        occupyTableSql,
                        new { TableId = tableId.Value },
                        transaction
                    );

                    if (affected == 0)
                        throw new Exception("Bàn đã được sử dụng.");
                }

                transaction.Commit();
                return (orderId, totalAmount);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> AddByStaffAsync(StaffCreateOrderDTO dto, int userId)
        {
            var result = await AddInternalAsync(
                dto.OrderDate,
                dto.TableId,
                dto.Status,
                userId,
                dto.CustomerId,
                dto.Note,
                dto.Details
            );

            await _hubContext.Clients.All.SendAsync("ReceiveNewOrder", new
            {
                OrderId = result.OrderId,
                TableId = dto.TableId,
                UserId = userId,
                CustomerId = dto.CustomerId,
                Status = dto.Status ?? "Pending",
                TotalAmount = result.TotalAmount,
                Source = "Staff",
                Message = "Có đơn hàng mới do nhân viên tạo"
            });

            return result.OrderId;
        }

        public async Task<int> AddByCustomerAsync(CustomerCreateOrderDTO dto, int customerId)
        {
            var result = await AddInternalAsync(
                dto.OrderDate,
                dto.TableId,
                null,
                null,
                customerId,
                dto.Note,
                dto.Details
            );

            await _hubContext.Clients.All.SendAsync("ReceiveNewOrder", new
            {
                OrderId = result.OrderId,
                TableId = dto.TableId,
                UserId = (int?)null,
                CustomerId = customerId,
                Status = "Pending",
                TotalAmount = result.TotalAmount,
                Source = "Customer",
                Message = "Có đơn hàng mới do khách hàng tạo"
            });

            return result.OrderId;
        }

        // =========================
        // UPDATE - KHÔNG SHIP
        // - Không cho sửa nếu đã Completed/Cancelled
        // - Tính lại total từ DB price
        // - Nếu chuyển Completed/Cancelled thì giải phóng bàn (lấy table_id từ DB)
        // =========================
        public async Task<int> UpdateAsync(OrderUpdateDTO dto)
        {
            if (_db.State != ConnectionState.Open) _db.Open();

            if (dto.Details == null || dto.Details.Count == 0)
                throw new ArgumentException("Order phải có ít nhất 1 sản phẩm.");

            if (dto.Details.Any(d => d.Quantity <= 0))
                throw new ArgumentException("Quantity phải >= 1.");

            using var transaction = _db.BeginTransaction();
            try
            {
                var meta = await _db.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT status, table_id, customer_id FROM orders WHERE order_id = @id",
                    new { id = dto.OrderId },
                    transaction);

                if (meta == null)
                    throw new Exception("Không tìm thấy đơn hàng.");

                string oldStatus = (string)meta.status;
                int? tableIdInDb = (int?)meta.table_id;
                int? customerIdInDb = (int?)meta.customer_id;

                // chặn sửa khi kết thúc
                if (oldStatus == "Completed" || oldStatus == "Cancelled")
                    throw new InvalidOperationException("Đơn hàng đã kết thúc, không thể cập nhật.");

                bool justCompleted = oldStatus != "Completed" && dto.Status == "Completed";

                // 1) Update thông tin chung (không ship)
                const string updateOrderSql = @" UPDATE orders SET
                                                        status = @Status,
                                                        note = @Note,
                                                        table_id = @TableId,
                                                        customer_id = @CustomerId
                                                    WHERE order_id = @OrderId;";

                await _db.ExecuteAsync(updateOrderSql, new
                {
                    dto.Status,
                    dto.Note,
                    dto.TableId,
                    CustomerId = dto.CustomerId ?? customerIdInDb,
                    dto.OrderId
                }, transaction);

                // 2) Xóa details cũ
                await _db.ExecuteAsync(
                    "DELETE FROM order_details WHERE order_id = @OrderId",
                    new { dto.OrderId },
                    transaction);

                // 3) Insert details mới + tính total
                const string insertDetailSql = @"INSERT INTO order_details (order_id, product_id, quantity, unit_price, subtotal)
                                                 VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @Subtotal);";

                decimal total = 0m;

                foreach (var item in dto.Details)
                {
                    var price = await _db.ExecuteScalarAsync<decimal>(
                        "SELECT price FROM products WHERE product_id = @id",
                        new { id = item.ProductId },
                        transaction);

                    if (price <= 0)
                        throw new Exception("Sản phẩm không hợp lệ.");

                    var subtotal = item.Quantity * price;
                    total += subtotal;

                    await _db.ExecuteAsync(insertDetailSql, new
                    {
                        OrderId = dto.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = price,
                        Subtotal = subtotal
                    }, transaction);
                }

                // 4) Update total_amount
                await _db.ExecuteAsync(
                    "UPDATE orders SET total_amount = @total WHERE order_id = @id",
                    new { total, id = dto.OrderId },
                    transaction);

                // 5) Tích điểm chỉ khi completed lần đầu (nếu có customer)
                if (justCompleted)
                {
                    int earnedPoints = (int)(total / 10000m);
                    if (earnedPoints > 0)
                    {
                        int? customerId = dto.CustomerId ?? customerIdInDb;
                        if (customerId.HasValue)
                        {
                            await _db.ExecuteAsync(
                                @"UPDATE customers
                                  SET loyalty_points = loyalty_points + @Points
                                  WHERE customer_id = @CustomerId",
                                new { Points = earnedPoints, CustomerId = customerId.Value },
                                transaction);
                        }
                    }
                }

                // 6) Nếu completed/cancelled => trả bàn (lấy tableId từ DB)
                if (dto.Status == "Completed" || dto.Status == "Cancelled")
                {
                    if (tableIdInDb.HasValue)
                    {
                        await _db.ExecuteAsync(
                            "UPDATE tables SET status = 'Available' WHERE table_id = @TableId",
                            new { TableId = tableIdInDb.Value },
                            transaction);
                    }
                }

                transaction.Commit();
                return 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            const string sql = "UPDATE orders SET status = @status WHERE order_id = @id";
            return await _db.ExecuteAsync(sql, new { id, status });
        }

        // =========================
        // DELETE
        // (DB nên có ON DELETE CASCADE ở order_details, nhưng vẫn có thể xóa an toàn)
        // =========================
        public async Task<int> DeleteAsync(int id)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            using var transaction = _db.BeginTransaction();

            try
            {
                const string getOrderSql = @"
            SELECT order_id AS OrderId, table_id AS TableId
            FROM orders
            WHERE order_id = @Id;
        ";

                var order = await _db.QueryFirstOrDefaultAsync<dynamic>(
                    getOrderSql,
                    new { Id = id },
                    transaction
                );

                if (order == null)
                {
                    transaction.Rollback();
                    return 0;
                }

                int? tableId = order.TableId == null ? (int?)null : (int)order.TableId;

                const string deleteDetailsSql = @"
            DELETE FROM order_details
            WHERE order_id = @Id;
        ";

                await _db.ExecuteAsync(
                    deleteDetailsSql,
                    new { Id = id },
                    transaction
                );

                const string deleteOrderSql = @"
            DELETE FROM orders
            WHERE order_id = @Id;
        ";

                int rows = await _db.ExecuteAsync(
                    deleteOrderSql,
                    new { Id = id },
                    transaction
                );

                if (tableId.HasValue)
                {
                    const string updateTableSql = @"
                UPDATE tables
                SET status = 'Available'
                WHERE table_id = @TableId;
            ";

                    await _db.ExecuteAsync(
                        updateTableSql,
                        new { TableId = tableId.Value },
                        transaction
                    );
                }

                transaction.Commit();
                return rows;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // =========================
        // SEARCH (tìm theo status)
        // =========================
        public async Task<IEnumerable<OrderDisplayDTO>> SearchAsync(string key)
        {
            const string sql = @"SELECT 
                                    o.order_id AS Id,
                                    o.created_at AS OrderDate,
                                    o.total_amount AS TotalAmount,
                                    o.status AS Status,
                                    o.table_id AS TableId,
                                    o.user_id AS UserId,
                                    o.customer_id AS CustomerId,
                                    o.note AS Note,
                                    u.full_name AS CreatorFullName
                                FROM orders o
                                LEFT JOIN users u ON o.user_id = u.user_id
                                WHERE o.status LIKE @key
                                ORDER BY o.created_at DESC;";

            return await _db.QueryAsync<OrderDisplayDTO>(sql, new { key = "%" + key + "%" });
        }

        public async Task<IEnumerable<OrderDisplayDTO>> GetOrdersByTableAsync(int tableId)
        {
            string sql = @"SELECT o.order_id AS Id, o.created_at AS OrderDate, o.total_amount, o.status, o.table_id 
                           FROM orders o WHERE o.table_id = @tableId";
            return await _db.QueryAsync<OrderDisplayDTO>(sql, new { tableId });
        }
    }
}