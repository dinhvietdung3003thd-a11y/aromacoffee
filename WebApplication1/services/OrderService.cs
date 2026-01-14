using Dapper;
using System.Data;
using WebApplication1.DTOs.order;
using WebApplication1.services.interfaces;

namespace WebApplication1.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDbConnection _db;

        public OrderService(IDbConnection db)
        {
            _db = db;
        }

        // 1. Lấy toàn bộ danh sách đơn hàng kèm tên người tạo
        public async Task<IEnumerable<OrderDisplayDTO>> GetAllAsync()
        {
            string sql = @"SELECT o.*, a.FullName AS CreatorFullName 
                           FROM orders o 
                           LEFT JOIN accounts a ON o.CreatedBy = a.Username";
            return await _db.QueryAsync<OrderDisplayDTO>(sql);
        }

        // 2. Lấy chi tiết đơn hàng theo Id
        public async Task<OrderDisplayDTO?> GetByIdAsync(int id)
        {
            string sql = @"SELECT o.*, a.FullName AS CreatorFullName 
                           FROM orders o 
                           LEFT JOIN accounts a ON o.CreatedBy = a.Username 
                           WHERE o.Id = @id";
            return await _db.QueryFirstOrDefaultAsync<OrderDisplayDTO>(sql, new { id });
        }

        // 3. Tạo mới một đơn hàng
        public async Task<int> AddAsync(OrderDisplayDTO dto)
        {
            string sql = @"INSERT INTO orders (OrderDate, TotalAmount, CreatedBy, TableNumber, Status) 
                           VALUES (@OrderDate, @TotalAmount, @CreatorFullName, @TableNumber, @Status);
                           SELECT LAST_INSERT_ID();";

            // Lưu ý: Trong thực tế, 'CreatedBy' nên lấy từ Username của session đăng nhập
            var id = await _db.ExecuteScalarAsync<int>(sql, dto);
            dto.Id = id;
            return id;
        }

        // 4. Cập nhật thông tin đơn hàng
        public async Task<int> UpdateAsync(OrderDisplayDTO dto)
        {
            string sql = @"UPDATE orders 
                           SET TotalAmount = @TotalAmount, 
                               TableNumber = @TableNumber, 
                               Status = @Status 
                           WHERE Id = @Id";
            return await _db.ExecuteAsync(sql, dto);
        }

        // 5. Xóa đơn hàng
        public async Task<int> DeleteAsync(int id)
        {
            string sql = "DELETE FROM orders WHERE Id = @id";
            return await _db.ExecuteAsync(sql, new { id });
        }

        // 6. Tìm kiếm đơn hàng theo trạng thái hoặc số bàn
        public async Task<IEnumerable<OrderDisplayDTO>> SearchAsync(string keyword)
        {
            string sql = @"SELECT o.*, a.FullName AS CreatorFullName 
                           FROM orders o 
                           LEFT JOIN accounts a ON o.CreatedBy = a.Username 
                           WHERE o.Status LIKE @key OR CAST(o.TableNumber AS CHAR) LIKE @key";
            return await _db.QueryAsync<OrderDisplayDTO>(sql, new { key = $"%{keyword}%" });
        }

        // 7. Cập nhật riêng trạng thái đơn hàng (Phương thức mở rộng của IOrderService)
        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            string sql = "UPDATE orders SET Status = @status WHERE Id = @id";
            return await _db.ExecuteAsync(sql, new { id, status });
        }

        // 8. Lấy danh sách đơn hàng theo số bàn
        public async Task<IEnumerable<OrderDisplayDTO>> GetOrdersByTableAsync(int tableNumber)
        {
            string sql = @"SELECT o.*, a.FullName AS CreatorFullName 
                           FROM orders o 
                           LEFT JOIN accounts a ON o.CreatedBy = a.Username 
                           WHERE o.TableNumber = @tableNumber";
            return await _db.QueryAsync<OrderDisplayDTO>(sql, new { tableNumber });
        }
    }
}