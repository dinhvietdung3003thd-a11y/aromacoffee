using Dapper;
using System.Data;
using WebApplication1.DTOs.order;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class OrderService : IOrderService
    {
        private readonly IDbConnection _db;
        public OrderService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<OrderDisplayDTO>> GetAllAsync()
        {
            string sql = @"SELECT o.order_id AS Id, o.created_at AS OrderDate, o.total_amount, 
                                   o.status, o.table_id, u.full_name AS CreatorFullName 
                           FROM orders o 
                           LEFT JOIN users u ON o.user_id = u.user_id";
            return await _db.QueryAsync<OrderDisplayDTO>(sql);
        }

        public async Task<OrderDisplayDTO?> GetByIdAsync(int id)
        {
            // Lấy thông tin đơn hàng
            string orderSql = "SELECT * FROM orders WHERE order_id = @id";
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

            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    // 1. Lưu thông tin đơn hàng chính vào bảng orders
                    string orderSql = @"INSERT INTO orders (user_id, table_id, total_amount, status, note) 
                               VALUES (@UserId, @TableId, @TotalAmount, @Status, @Note);
                               SELECT LAST_INSERT_ID();";

                    var orderId = await _db.ExecuteScalarAsync<int>(orderSql, dto, transaction);

                    // 2. Lưu danh sách chi tiết món ăn vào bảng order_details
                    string detailSql = @"INSERT INTO order_details (order_id, product_id, quantity, unit_price, subtotal) 
                                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @Subtotal)";

                    foreach (var item in dto.Details)
                    {
                        await _db.ExecuteAsync(detailSql, new
                        {
                            OrderId = orderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Subtotal = item.Quantity * item.UnitPrice
                        }, transaction);
                    }

                    // 3.TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI BÀN
                    // Nếu đơn hàng có gắn với một bàn cụ thể (TableId không null)
                    if (dto.TableId.HasValue && dto.TableId > 0)
                    {
                        // Cập nhật trạng thái bàn thành 'Occupied' (Có người) trong bảng tables
                        string updateTableSql = "UPDATE tables SET status = 'Occupied' WHERE table_id = @TableId";
                        await _db.ExecuteAsync(updateTableSql, new { TableId = dto.TableId }, transaction);
                    }

                    // 4. Xác nhận hoàn tất mọi thay đổi nếu không có lỗi
                    transaction.Commit();
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
                    // 1. Cập nhật thông tin chung của đơn hàng
                    string updateOrderSql = @"UPDATE orders 
                                     SET total_amount = @TotalAmount, 
                                         status = @Status, 
                                         table_id = @TableId,
                                         note = @Note
                                     WHERE order_id = @OrderId"; // Giả sử dto có OrderId hoặc truyền thêm vào

                    await _db.ExecuteAsync(updateOrderSql, dto, transaction);

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
                    //4. LOGIC TRỪ KHO TỰ ĐỘNG KHI HOÀN TẤT
                    if (dto.Status == "Completed")
                    {
                        // Lấy danh sách sản phẩm và số lượng trong đơn hàng hiện tại
                        string getItemsSql = "SELECT product_id, quantity FROM order_details WHERE order_id = @OrderId";
                        var items = await _db.QueryAsync(getItemsSql, new { dto.OrderId }, transaction);

                        foreach (var item in items)
                        {
                            // Lấy công thức cho từng sản phẩm
                            string getRecipeSql = "SELECT inventory_id, quantity_needed FROM recipes WHERE product_id = @ProductId";
                            var ingredients = await _db.QueryAsync(getRecipeSql, new { ProductId = item.product_id }, transaction);

                            foreach (var ing in ingredients)
                            {
                                // Tính toán tổng lượng cần trừ: Total = Số lượng món * Lượng nguyên liệu/món
                                decimal totalDeduct = (decimal)item.quantity * (decimal)ing.quantity_needed;

                                // Ghi nhật ký xuất kho (Export)
                                string logSql = @"INSERT INTO inventory_transactions 
                                         (inventory_id, transaction_type, quantity, user_date, user_id, note) 
                                         VALUES (@InvId, 'Export', @Qty, NOW(), @UserId, @Note)";

                                await _db.ExecuteAsync(logSql, new
                                {
                                    InvId = ing.inventory_id,
                                    Qty = totalDeduct,
                                    UserId = dto.UserId, // ID nhân viên thực hiện
                                    Note = $"Xuất kho tự động cho đơn hàng #{dto.OrderId}"
                                }, transaction);

                                // Cập nhật số lượng tồn kho thực tế
                                string updateInvSql = @"UPDATE inventory 
                                               SET quantity_in_stock = quantity_in_stock - @Qty 
                                               WHERE inventory_id = @InvId";
                                await _db.ExecuteAsync(updateInvSql, new { Qty = totalDeduct, InvId = ing.inventory_id }, transaction);
                            }
                        }
                    }
                    //5. LOGIC TÍCH ĐIỂM TỰ ĐỘNG
                    if (dto.Status == "Completed" && dto.CustomerId.HasValue)
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

                    //6. LOGIC TỰ ĐỘNG GIẢI PHÓNG BÀN
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

        public async Task<int> UpdateStatusAsync(int id, string status) =>
            await _db.ExecuteAsync("UPDATE orders SET status = @status WHERE order_id = @id", new { id, status });

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