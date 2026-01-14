using Dapper;
using System.Data;
using WebApplication1.DTOs.categorys;
using WebApplication1.Models;
using WebApplication1.Services.interfaces;

namespace WebApplication1.Services
{
    public class CatagoryService : ICatagoryService
    {
        private readonly IDbConnection _db;

        // Inject IDbConnection từ Program.cs để kết nối MySQL
        public CatagoryService(IDbConnection db)
        {
            _db = db;
        }

        // 1. Lấy toàn bộ danh sách từ bảng categories
        public async Task<IEnumerable<CatagoryDTO>> GetAllAsync()
        {
            string sql = "SELECT * FROM categories";
            return await _db.QueryAsync<CatagoryDTO>(sql);
        }

        // 2. Lấy chi tiết một danh mục theo Id
        public async Task<CatagoryDTO?> GetByIdAsync(int id)
        {
            string sql = "SELECT * FROM categories WHERE Id = @id";
            return await _db.QueryFirstOrDefaultAsync<CatagoryDTO>(sql, new { id });
        }

        // 3. Thêm mới danh mục vào DB
        public async Task<int> AddAsync(CatagoryDTO category)
        {
            // MySQL sẽ tự tăng Id nên chúng ta không chèn cột Id vào đây
            string sql = "INSERT INTO categories (Name, Description) VALUES (@Name, @Description)";
            return await _db.ExecuteAsync(sql, category);
        }

        // 4. Cập nhật thông tin danh mục
        public async Task<int> UpdateAsync(CatagoryDTO category)
        {
            string sql = "UPDATE categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            return await _db.ExecuteAsync(sql, category);
        }

        // 5. Xóa danh mục khỏi DB
        public async Task<int> DeleteAsync(int id)
        {
            string sql = "DELETE FROM categories WHERE Id = @id";
            return await _db.ExecuteAsync(sql, new { id });
        }

        // 6. Tìm kiếm danh mục theo tên
        public async Task<IEnumerable<CatagoryDTO>> SearchAsync(string keyword)
        {
            string sql = "SELECT * FROM categories WHERE Name LIKE @key";
            return await _db.QueryAsync<CatagoryDTO>(sql, new { key = $"%{keyword}%" });
        }
    }
}