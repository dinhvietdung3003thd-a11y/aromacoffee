using Dapper;
using System.Data;
using WebApplication1.DTOs.product;
using WebApplication1.Services.interfaces;

namespace WebApplication1.Services
{
    public class ProductService : IProductService
    {
        private readonly IDbConnection _db;
        public ProductService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            // JOIN với bảng categories để lấy CategoryName
            string sql = @"SELECT p.*, c.Name AS CategoryName 
                           FROM products p 
                           LEFT JOIN categories c ON p.CategoryId = c.Id";
            return await _db.QueryAsync<ProductDTO>(sql);
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            string sql = @"SELECT p.*, c.Name AS CategoryName 
                           FROM products p 
                           LEFT JOIN categories c ON p.CategoryId = c.Id 
                           WHERE p.Id = @id";
            return await _db.QueryFirstOrDefaultAsync<ProductDTO>(sql, new { id });
        }

        public async Task<int> AddAsync(ProductDTO dto)
        {
            // KHÔNG chèn Id vì DB tự tăng. 
            // Sau khi chèn, dùng LAST_INSERT_ID() để lấy ID vừa tạo.
            string sql = @"INSERT INTO products (Name, Price, ImageUrl, Status, CategoryId) 
                           VALUES (@Name, @Price, @ImageUrl, @Status, @CategoryId);
                           SELECT LAST_INSERT_ID();";

            var id = await _db.ExecuteScalarAsync<int>(sql, dto);
            dto.Id = id; // Gán ID mới vào DTO để trả về cho người dùng
            return id;
        }

        public async Task<int> UpdateAsync(ProductDTO dto)
        {
            string sql = @"UPDATE products 
                           SET Name=@Name, Price=@Price, ImageUrl=@ImageUrl, 
                               Status=@Status, CategoryId=@CategoryId 
                           WHERE Id=@Id";
            return await _db.ExecuteAsync(sql, dto);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _db.ExecuteAsync("DELETE FROM products WHERE Id=@id", new { id });
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string keyword)
        {
            string sql = @"SELECT p.*, c.Name AS CategoryName 
                           FROM products p 
                           LEFT JOIN categories c ON p.CategoryId = c.Id 
                           WHERE p.Name LIKE @key";
            return await _db.QueryAsync<ProductDTO>(sql, new { key = $"%{keyword}%" });
        }
    }
}