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
            string sql = @"SELECT p.*, c.name AS CategoryName 
                           FROM products p 
                           LEFT JOIN categories c ON p.category_id = c.category_id";
            return await _db.QueryAsync<ProductDTO>(sql);
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            string sql = @"SELECT p.*, c.name AS CategoryName 
                           FROM products p 
                           LEFT JOIN categories c ON p.category_id = c.category_id 
                           WHERE p.product_id = @id";
            return await _db.QueryFirstOrDefaultAsync<ProductDTO>(sql, new { id });
        }

        public async Task<int> AddAsync(ProductDTO dto)
        {
            string sql = @"INSERT INTO products (name, price, image_url, is_available, category_id) 
                           VALUES (@Name, @Price, @ImageUrl, @IsAvailable, @CategoryId);
                           SELECT LAST_INSERT_ID();";
            var id = await _db.ExecuteScalarAsync<int>(sql, dto);
            dto.ProductId = id;
            return id;
        }

        public async Task<int> UpdateAsync(ProductDTO dto)
        {
            string sql = @"UPDATE products 
                           SET name=@Name, price=@Price, image_url=@ImageUrl, 
                               is_available=@IsAvailable, category_id=@CategoryId 
                           WHERE product_id=@ProductId";
            return await _db.ExecuteAsync(sql, dto);
        }

        public async Task<int> DeleteAsync(int id) =>
            await _db.ExecuteAsync("DELETE FROM products WHERE product_id=@id", new { id });

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string keyword)
        {
            string sql = @"SELECT p.*, c.name AS CategoryName 
                           FROM products p 
                           LEFT JOIN categories c ON p.category_id = c.category_id 
                           WHERE p.name LIKE @key";
            return await _db.QueryAsync<ProductDTO>(sql, new { key = $"%{keyword}%" });
        }
    }
}