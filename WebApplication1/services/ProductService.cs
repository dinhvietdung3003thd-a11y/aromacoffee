using Dapper;
using Nest;
using System.Data;
using WebApplication1.DTOs.product;
using WebApplication1.Services.interfaces;

namespace WebApplication1.Services
{
    public class ProductService : IProductService
    {
        private readonly IDbConnection _db;
        private readonly IElasticClient _elasticClient;
        public ProductService(IDbConnection db, IElasticClient elasticClient)
        {
            _db = db;
            _elasticClient = elasticClient;
        }

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

            // Đẩy dữ liệu sang Elasticsearch
            await _elasticClient.IndexDocumentAsync(dto);
            return id;
        }

        public async Task<int> UpdateAsync(ProductDTO dto)
        {
            string sql = @"UPDATE products 
                           SET name=@Name, price=@Price, image_url=@ImageUrl, 
                               is_available=@IsAvailable, category_id=@CategoryId 
                           WHERE product_id=@ProductId";
            await _elasticClient.IndexDocumentAsync(dto);
            return await _db.ExecuteAsync(sql, dto);
        }

        public async Task<int> DeleteAsync(int id)
        {
            var result = await _db.ExecuteAsync("DELETE FROM products WHERE product_id=@id", new { id });
            await _elasticClient.DeleteAsync<ProductDTO>(id);
            return result;
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string keyword)
        {
            var searchResponse = await _elasticClient.SearchAsync<ProductDTO>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(p => p.Name, boost: 2) // Ưu tiên tìm theo tên món
                            .Field(p => p.CategoryName)   // Tìm cả trong danh mục
                        )
                        .Query(keyword)
                        .Fuzziness(Fuzziness.Auto) // Tự động sửa lỗi chính tả
                    )
                )
            );

            return searchResponse.Documents;
        }
    }
}