using Dapper;
using System.Data;
using WebApplication1.DTOs;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    public class ProductService : IProductService
    {
        private readonly IDbConnection _db;

        public ProductService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            // Logic "so khớp" lấy tên danh mục từ database
            string sql = @"SELECT p.*, c.Name AS CategoryName 
                           FROM products p 
                           JOIN categories c ON p.CategoryId = c.Id";

            return await _db.QueryAsync<ProductDTO>(sql);
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            string sql = @"SELECT p.*, c.Name AS CategoryName 
                           FROM products p 
                           JOIN categories c ON p.CategoryId = c.Id 
                           WHERE p.Id = @id";

            return await _db.QueryFirstOrDefaultAsync<ProductDTO>(sql, new { id });
        }
    }
}s