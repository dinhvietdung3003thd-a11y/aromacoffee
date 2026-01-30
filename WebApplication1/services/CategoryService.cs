using Dapper;
using System.Data;
using WebApplication1.DTOs.categorys;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDbConnection _db;
        public CategoryService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync() =>
            await _db.QueryAsync<CategoryDTO>("SELECT * FROM categories");

        public async Task<CategoryDTO?> GetByIdAsync(int id) =>
            await _db.QueryFirstOrDefaultAsync<CategoryDTO>("SELECT * FROM categories WHERE category_id = @id", new { id });

        public async Task<int> AddAsync(CategoryDTO category)
        {
            string sql = "INSERT INTO categories (name, description) VALUES (@Name, @Description)";
            return await _db.ExecuteAsync(sql, category);
        }

        public async Task<int> UpdateAsync(CategoryDTO category)
        {
            string sql = "UPDATE categories SET name = @Name, description = @Description WHERE category_id = @CategoryId";
            return await _db.ExecuteAsync(sql, category);
        }

        public async Task<int> DeleteAsync(int id) =>
            await _db.ExecuteAsync("DELETE FROM categories WHERE category_id = @id", new { id });

        public async Task<IEnumerable<CategoryDTO>> SearchAsync(string keyword) =>
            await _db.QueryAsync<CategoryDTO>("SELECT * FROM categories WHERE name LIKE @key", new { key = $"%{keyword}%" });
    }
}