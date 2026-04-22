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

        public async Task<IEnumerable<CategoryDTO>> GetAllDisplayAsync() =>
            await _db.QueryAsync<CategoryDTO>("SELECT * FROM categories");

        public async Task<CategoryDTO?> GetByIdAsync(int id) =>
            await _db.QueryFirstOrDefaultAsync<CategoryDTO>("SELECT * FROM categories WHERE category_id = @id", new { id });

        public async Task<int> AddAsync(CategoryCreateDTO dto)
        {

            string sql = @"INSERT INTO categories (name, description) VALUES (@Name, @Description);
                           SELECT LAST_INSERT_ID();
    ";

            int newId = await _db.ExecuteScalarAsync<int>(sql, dto);
            return newId;
        }

        public async Task<int> UpdateAsync(int id, CategoryDTO dto)
        {
            string sql = @"UPDATE categories
                   SET name = @Name,
                       description = @Description
                   WHERE category_id = @Id";

            return await _db.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.Description
            });
        }

        public async Task<int> DeleteAsync(int id) =>
            await _db.ExecuteAsync("DELETE FROM categories WHERE category_id = @id", new { id });

        public async Task<IEnumerable<CategoryDTO>> SearchAsync(string keyword) =>
            await _db.QueryAsync<CategoryDTO>("SELECT * FROM categories WHERE name LIKE @key", new { key = $"%{keyword}%" });
    }
}