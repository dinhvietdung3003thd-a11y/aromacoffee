using Dapper;
using System.Data;
using WebApplication1.DTOs.order;
using WebApplication1.services.interfaces;
using Microsoft.AspNetCore.SignalR;
using WebApplication1.Hubs;

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

        public async Task<IEnumerable<OrderDisplayDTO>> GetAllAsync()
        {
            string sql = @" SELECT o.order_id AS Id, o.created_at AS OrderDate, o.total_amount,
                                   o.status, o.table_id, o.shipping_address, o.shipping_fee, o.distance_km, 
                                   u1.full_name AS CreatorFullName, u2.full_name AS ShipperName
                                   FROM orders o 
                            LEFT JOIN users u1 ON o.user_id = u1.user_id
                            LEFT JOIN users u2 ON o.shipper_id = u2.user_id
                            ORDER BY o.created_at DESC";
            return await _db.QueryAsync<OrderDisplayDTO>(sql);
        }

        public async Task<OrderDisplayDTO?> GetByIdAsync(int id)
        {
            // Lấy thông tin đơn hàng
            string orderSql = @" SELECT o.order_id AS Id, o.created_at AS OrderDate, o.total_amount,
                                        o.status, o.table_id, o.shipping_address, o.shipping_fee, o.distance_km,
                                        u.full_name AS CustomerFullName, u2.full_name AS ShipperName
                                        FROM orders o
                                 LEFT JOIN users u ON o.user_id = u.user_id
                                 LEFT JOIN users u2 ON o.shipper_id = u2.user_id
                                 WHERE o.order_id = @id";
            var order = await _db.QueryFirstOrDefaultAsync<OrderDisplayDTO>(orderSql, new { id });

            if (order != null)
            {
                // Lấy danh sách các món đã đặt kèm tên món
                string detailSql = @"SELECT od.*, p.name AS ProductName 
                             FROM order_details od
                             JOIN products p ON od.product_id = p.product_id
                             WHERE od.order_id = @id";
                var details = await _db.QueryAsync<OrderDetailDTO>(detailSql, new { id });

                order.Details = details.ToList();
            }
            return order;
        }

        public async Task<int> AddAsync(OrderCreateDTO dto)
        {
            // Đảm bảo kết nối được mở trước khi bắt đầu Transaction
            if (_db.State != ConnectionState.Open) _db.Open();

            if (dto.Details == null || dto.Details.Count == 0)
                throw new ArgumentException("Order phải có ít nhất 1 sản phẩm.");

            if (dto.Details.Any(d => d.Quantity <= 0))
                throw new ArgumentException("Quantity phải lớn hơn 0.");

            bool isDelivery = !string.IsNullOrWhiteSpace(dto.ShippingAddress);

            if (isDelivery)
            {
                if (dto.ShippingFee is < 0) throw new ArgumentException("ShippingFee không hợp lệ.");
                if (dto.DistanceKm is < 0) throw new ArgumentException("DistanceKm không hợp lệ.");
            }

            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    decimal pricePerKm = 5000;
                    dto.ShippingFee = (decimal)(dto.DistanceKm ?? 0) * pricePerKm;
                    // 1. Lưu thông tin đơn hàng chính vào bảng orders
                    string sql = @"INSERT INTO orders (
                                    created_at, total_amount, user_id, table_id, 
                                    status, customer_id, note, 
                                    shipping_address, lat, lng, distance_km, shipping_fee
                                 ) 
                                 VALUES (
                                    @OrderDate, @TotalAmount, @UserId, @TableId, 
                                    @Status, @CustomerId, @Note, 
                                    @ShippingAddress, @Lat, @Lng, @DistanceKm, @ShippingFee
                                 );
                                 SELECT LAST_INSERT_ID();";

                    var orderId = await _db.ExecuteScalarAsync<int>(sql, dto, transaction);

                    // 2. Lưu danh sách chi tiết món ăn vào bảng order_details
                    string detailSql = @"INSERT INTO order_details (order_id, product_id, quantity, unit_price, subtotal) 
                                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @Subtotal)";

                    foreach (var item in dto.Details)
                    {
                        // Lấy giá thật từ DB
                        var price = await _db.ExecuteScalarAsync<decimal>(
                            "SELECT price FROM products WHERE product_id = @id",
                            new { id = item.ProductId },
                            transaction);

                        if (price <= 0)
                            throw new Exception("Sản phẩm không hợp lệ.");

                        var subtotal = price * item.Quantity;

                        await _db.ExecuteAsync(detailSql, new
                        {
                            OrderId = orderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = price,
                            Subtotal = subtotal
                        }, transaction);
                    }

                    // 3.TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI BÀN
                    // Nếu đơn hàng có gắn với một bàn cụ thể (TableId không null)
                    if (dto.TableId.HasValue && dto.TableId > 0)
                    {
                        // Cập nhật trạng thái bàn thành 'Occupied' (Có người) trong bảng tables
                        string updateTableSql = "UPDATE tables SET status = 'Occupied' WHERE table_id = @TableId AND status='Available'";
                        var rowsAffected = await _db.ExecuteAsync(updateTableSql, new { TableId = dto.TableId }, transaction);
                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            throw new Exception("Bàn này đã có người đặt.");
                        }
                    }

                    // 4. Xác nhận hoàn tất mọi thay đổi nếu không có lỗi
                    transaction.Commit();
                    // PHẦN QUAN TRỌNG: Phát tín hiệu cho toàn hệ thống
                    await _hubContext.Clients.All.SendAsync("NewOrderAlert", new
                    {
                        id = orderId,
                        address = dto.ShippingAddress,
                        fee = dto.ShippingFee,
                        lat = dto.Lat,
                        lng = dto.Lng
                    });
                    return orderId;
                }
                catch (Exception)
                {
                    // Nếu có bất kỳ lỗi nào xảy ra, toàn bộ (Order, Details, Table Status) sẽ được khôi phục lại ban đầu
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Thay đổi tham số truyền vào thành OrderCreateDTO để có danh sách Details mới
        public async Task<int> UpdateAsync(OrderUpdateDTO dto)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    var oldStatus = await _db.ExecuteScalarAsync<string>(
                         "SELECT status FROM orders WHERE order_id = @OrderId",
                         new { dto.OrderId },
                         transaction
                         );

                    bool justCompleted = oldStatus != "Completed" && dto.Status == "Completed";

                    // 0) Chặn update nếu trạng thái không đổi
                    if (string.Equals(oldStatus, dto.Status, StringComparison.OrdinalIgnoreCase))
                    {
                        transaction.Rollback();
                        throw new InvalidOperationException("Đơn hàng đã ở trạng thái này rồi.");
                    }

                    // 1) Chặn thay đổi trạng thái từ trạng thái kết thúc
                    if (oldStatus == "Completed" || oldStatus == "Cancelled")
                    {
                        transaction.Rollback();
                        throw new InvalidOperationException("Đơn hàng đã kết thúc, không thể đổi trạng thái nữa.");
                    }

                    // 2) Chỉ cho phép Pending -> Completed/Cancelled (vì DB của bạn chỉ có 3 trạng thái)
                    if (oldStatus == "Pending" && dto.Status != "Completed" && dto.Status != "Cancelled")
                    {
                        transaction.Rollback();
                        throw new InvalidOperationException("Chỉ cho phép chuyển Pending -> Completed hoặc Cancelled.");
                    }

                    // 1. Cập nhật thông tin chung của đơn hàng
                    string sql = @"UPDATE orders SET 
                        status = @Status, 
                        note = @Note, 
                        shipping_address = @ShippingAddress, 
                        lat = @Lat, 
                        lng = @Lng, 
                        distance_km = @DistanceKm, 
                        shipping_fee = @ShippingFee,
                        shipper_id = @ShipperId
                    WHERE order_id = @OrderId";

                    await _db.ExecuteAsync(sql, dto, transaction);

                    // 2. Xóa toàn bộ chi tiết cũ của đơn hàng này
                    await _db.ExecuteAsync("DELETE FROM order_details WHERE order_id = @OrderId", new { OrderId = dto.OrderId }, transaction);

                    // 3. Chèn lại danh sách chi tiết món ăn mới
                    string insertDetailSql = @"INSERT INTO order_details (order_id, product_id, quantity, unit_price, subtotal) 
                                       VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @Subtotal)";

                    foreach (var item in dto.Details)
                    {
                        await _db.ExecuteAsync(insertDetailSql, new
                        {
                            OrderId = dto.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Subtotal = item.Quantity * item.UnitPrice
                        }, transaction);
                    }
                    //4. LOGIC TÍCH ĐIỂM TỰ ĐỘNG
                    if (justCompleted)
                    {
                        // Tính số điểm tích lũy (1% tổng tiền hoặc 10k = 1 điểm)
                        int earnedPoints = (int)((dto.TotalAmount ?? 0) / 10000);

                        if (earnedPoints > 0)
                        {
                            // Cộng điểm vào bảng customers
                            string updatePointsSql = @"UPDATE customers 
                                               SET loyalty_points = loyalty_points + @Points 
                                               WHERE customer_id = @CustomerId";

                            await _db.ExecuteAsync(updatePointsSql, new
                            {
                                Points = earnedPoints,
                                CustomerId = dto.CustomerId
                            }, transaction);
                        }
                    }

                    //5. LOGIC TỰ ĐỘNG GIẢI PHÓNG BÀN
                    // Nếu trạng thái chuyển thành 'Completed' (Hoàn thành) hoặc 'Cancelled' (Hủy)
                    if (dto.Status == "Completed" || dto.Status == "Cancelled")
                    {
                        if (dto.TableId.HasValue)
                        {
                            // Chuyển bàn về trạng thái 'Available' (Trống)
                            await _db.ExecuteAsync("UPDATE tables SET status = 'Available' WHERE table_id = @TableId",
                                                   new { TableId = dto.TableId }, transaction);
                        }
                    }

                    transaction.Commit();
                    return 1;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<int> DeleteAsync(int id) =>
            await _db.ExecuteAsync("DELETE FROM orders WHERE order_id = @id", new { id });

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            if (_db.State != ConnectionState.Open) _db.Open();

            using var transaction = _db.BeginTransaction();
            try
            {
                var oldStatus = await _db.ExecuteScalarAsync<string>(
                    "SELECT status FROM orders WHERE order_id = @id",
                    new { id },
                    transaction
                );

                if (oldStatus == null)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Không tìm thấy đơn hàng.");
                }

                // chặn update trùng
                if (string.Equals(oldStatus, status, StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Đơn hàng đã ở trạng thái này rồi.");
                }

                // chặn đổi từ trạng thái kết thúc
                if (oldStatus == "Completed" || oldStatus == "Cancelled")
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Đơn hàng đã kết thúc, không thể đổi trạng thái nữa.");
                }

                // chỉ cho Pending -> Completed/Cancelled
                if (oldStatus == "Pending" && status != "Completed" && status != "Cancelled")
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Chỉ cho phép chuyển Pending -> Completed hoặc Cancelled.");
                }

                // update có điều kiện theo oldStatus để chống 2 client bấm cùng lúc
                var rows = await _db.ExecuteAsync(
                    "UPDATE orders SET status = @status WHERE order_id = @id AND status = @oldStatus",
                    new { id, status, oldStatus },
                    transaction
                );

                if (rows == 0)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Trạng thái đã bị thay đổi bởi request khác, vui lòng thử lại.");
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

        public async Task<IEnumerable<OrderDisplayDTO>> GetOrdersByTableAsync(int tableId)
        {
            string sql = @"SELECT o.order_id AS Id, o.created_at AS OrderDate, o.total_amount, o.status, o.table_id 
                           FROM orders o WHERE o.table_id = @tableId";
            return await _db.QueryAsync<OrderDisplayDTO>(sql, new { tableId });
        }

        public async Task<IEnumerable<OrderDisplayDTO>> SearchAsync(string keyword)
        {
            string sql = "SELECT * FROM orders WHERE status LIKE @key";
            return await _db.QueryAsync<OrderDisplayDTO>(sql, new { key = $"%{keyword}%" });
        }
        public Task<int> AddAsync(OrderDisplayDTO entity)
        {
            // Hàm này bắt buộc phải có theo Interface nhưng chúng ta không dùng đến
            throw new NotImplementedException("Dùng AddAsync(OrderCreateDTO) để tạo đơn hàng.");
        }
        public Task<int> UpdateAsync(OrderDisplayDTO entity)
        {
            // Hàm này bắt buộc phải có theo Interface nhưng chúng ta không dùng đến
            throw new NotImplementedException("Dùng UpdateAsync(OrderCreateDTO) để tạo đơn hàng.");
        }

    }
}