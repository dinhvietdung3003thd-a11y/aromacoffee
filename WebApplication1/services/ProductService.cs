using Dapper;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Nest;
using System;
using System.Data;
using WebApplication1.DTOs.product;
using WebApplication1.Models;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class ProductService :   IProductService
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

        public async Task<int> AddAsync(ProductCreateDTO dto)
        {
            string sql = @"INSERT INTO products (name, price, image_url, is_available, category_id, description) 
                   VALUES (@Name, @Price, @ImageUrl, @IsAvailable, @CategoryId, @Description);
                   SELECT LAST_INSERT_ID();";

            var id = await _db.ExecuteScalarAsync<int>(sql, dto);

            var product = new Product
            {
                ProductId = id,
                Name = dto.Name,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.ImageUrl,
                IsAvailable = dto.IsAvailable,
                Description = dto.Description
            };

            await TryIndexProductAsync(product);
            return id;
        }

        public async Task<int> UpdateAsync(int id, ProductUpdateDTO dto)
        {
            string sql = @"UPDATE products 
                   SET name = @Name,
                       price = @Price,
                       image_url = @ImageUrl,
                       is_available = @IsAvailable,
                       category_id = @CategoryId,
                       description = @Description
                   WHERE product_id = @Id";

            var rows = await _db.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.Price,
                dto.ImageUrl,
                dto.IsAvailable,
                dto.CategoryId,
                dto.Description
            });

            if (rows > 0)
            {
                var product = new Product
                {
                    ProductId = id,
                    Name = dto.Name,
                    Price = dto.Price,
                    CategoryId = dto.CategoryId,
                    ImageUrl = dto.ImageUrl,
                    IsAvailable = dto.IsAvailable,
                    Description = dto.Description
                };

                await TryIndexProductAsync(product);
            }

            return rows;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM products WHERE product_id = @id",
                new { id });

            if (rows > 0)
            {
                await TryDeleteProductAsync(id);
            }

            return rows;
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string keyword)
        {
            string sql = @"SELECT p.*, c.name AS CategoryName 
                           FROM products p 
                           LEFT JOIN categories c ON p.category_id = c.category_id 
                           WHERE p.name LIKE @key";
            return await _db.QueryAsync<ProductDTO>(sql, new { key = $"%{keyword}%" });
        }

        // hàm search sử dụng Elasticsearch
        public async Task<IEnumerable<ProductDTO>> SearchProductsElasticAsync(string keyword)
        {
            var response = await _elasticClient.SearchAsync<Product>(s => s
                .Index("aroma_products")
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(p => p.Name)
                            .Field(p => p.Description)
                        )
                        .Query(keyword)
                        .Type(TextQueryType.BoolPrefix)
                    )
                )
            );

            if (!response.IsValid)
                return Enumerable.Empty<ProductDTO>();

            return response.Documents.Select(p => new ProductDTO
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                ImageUrl = p.ImageUrl,
                IsAvailable = p.IsAvailable,
                Description = p.Description
            });
        }   

        public async Task SyncProductsToElasticAsync()
        {
            var products = await _db.QueryAsync<Product>("SELECT * FROM products");

            foreach (var product in products)
            {
                await TryIndexProductAsync(product);
            }
        }
        private async Task TryIndexProductAsync(Product product)
        {
            try
            {
                await _elasticClient.IndexAsync(product, i => i
                    .Index("aroma_products")
                    .Id(product.ProductId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Elastic index error for ProductId {product.ProductId}: {ex.Message}");
            }
        }
        private async Task TryDeleteProductAsync(int productId)
        {
            try
            {
                await _elasticClient.DeleteAsync<Product>(productId, d => d
                    .Index("aroma_products"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Elastic delete error for ProductId {productId}: {ex.Message}");
            }
        }
    }
}