using Dapper;
using Nest;
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
        private readonly string _elasticIndexName;
        public ProductService(IDbConnection db, IElasticClient elasticClient, IConfiguration configuration)
        {
            _db = db;
            _elasticClient = elasticClient;
            _elasticIndexName = configuration["Elasticsearch:DefaultIndex"] ?? "aroma_products";
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
                .Index(_elasticIndexName)
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

        public async Task<IEnumerable<ProductIngredientAvailabilityDTO>> GetIngredientAvailabilityAsync(int? productId = null)
        {
            var products = (await _db.QueryAsync<ProductAvailabilityRow>(
                @"SELECT p.product_id AS ProductId, p.name AS ProductName
                  FROM products p
                  WHERE (@ProductId IS NULL OR p.product_id = @ProductId)
                  ORDER BY p.product_id ASC;",
                new { ProductId = productId })).ToList();

            if (products.Count == 0)
                return Enumerable.Empty<ProductIngredientAvailabilityDTO>();

            var productIds = products.Select(p => p.ProductId).ToArray();

            var recipeRows = (await _db.QueryAsync<ProductRecipeAvailabilityRow>(
                @"SELECT r.product_id AS ProductId,
                         r.inventory_id AS InventoryId,
                         r.quantity_needed AS QuantityNeeded,
                         i.quantity_in_stock AS QuantityInStock
                  FROM recipes r
                  JOIN inventory i ON i.inventory_id = r.inventory_id
                  WHERE r.product_id IN @ProductIds AND r.quantity_needed > 0;",
                new { ProductIds = productIds })).ToList();

            const int lowStockThreshold = 3;
            var result = new List<ProductIngredientAvailabilityDTO>();

            foreach (var product in products)
            {
                var productRecipes = recipeRows.Where(r => r.ProductId == product.ProductId).ToList();
                result.Add(BuildAvailability(product.ProductId, product.ProductName, productRecipes, lowStockThreshold));
            }

            return result;
        }

        private static ProductIngredientAvailabilityDTO BuildAvailability(
            int productId,
            string productName,
            List<ProductRecipeAvailabilityRow> recipes,
            int lowStockThreshold)
        {
            if (recipes.Count == 0)
            {
                return new ProductIngredientAvailabilityDTO
                {
                    ProductId = productId,
                    ProductName = productName,
                    EstimatedServingsLeft = 0,
                    Status = "not_tracked",
                    WarningMessage = $"Product '{productName}' has no recipe configured."
                };
            }

            int estimatedServings = recipes
                .Select(r => (int)Math.Floor(r.QuantityInStock / r.QuantityNeeded))
                .Min();

            int displayServings = Math.Max(0, estimatedServings);

            if (displayServings <= 0)
            {
                return new ProductIngredientAvailabilityDTO
                {
                    ProductId = productId,
                    ProductName = productName,
                    EstimatedServingsLeft = 0,
                    Status = "out_of_ingredients",
                    WarningMessage = $"Product '{productName}' is out of ingredients based on recorded stock."
                };
            }

            if (displayServings <= lowStockThreshold)
            {
                return new ProductIngredientAvailabilityDTO
                {
                    ProductId = productId,
                    ProductName = productName,
                    EstimatedServingsLeft = displayServings,
                    Status = "low_stock",
                    WarningMessage = $"Product '{productName}' is running low on ingredients, only enough for about {displayServings} more servings."
                };
            }

            return new ProductIngredientAvailabilityDTO
            {
                ProductId = productId,
                ProductName = productName,
                EstimatedServingsLeft = displayServings,
                Status = "available",
                WarningMessage = string.Empty
            };
        }

        private async Task TryIndexProductAsync(Product product)
        {
            try
            {
                await _elasticClient.IndexAsync(product, i => i
                    .Index(_elasticIndexName)
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
                    .Index(_elasticIndexName));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Elastic delete error for ProductId {productId}: {ex.Message}");
            }
        }

        private sealed class ProductAvailabilityRow
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
        }

        private sealed class ProductRecipeAvailabilityRow
        {
            public int ProductId { get; set; }
            public int InventoryId { get; set; }
            public decimal QuantityNeeded { get; set; }
            public decimal QuantityInStock { get; set; }
        }
    }
}
