using Dapper;
using System.Data;
using WebApplication1.DTOs.recipes;
using WebApplication1.services.interfaces;
namespace WebApplication1.services
{ 
    public class RecipeService : IRecipeService
    {
        private readonly IDbConnection _db;
        public RecipeService(IDbConnection db) => _db = db;

        // --- TRIỂN KHAI CÁC HÀM TỪ IBASE ---
        public async Task<RecipeDisplayDTO?> GetByIdAsync(int id)
        {
            string sql = @"SELECT r.recipe_id,
                          p.name AS ProductName,
                          i.name AS InventoryName,
                          r.quantity_needed AS QuantityNeeded,
                          i.unit
                   FROM recipes r
                   JOIN products p ON r.product_id = p.product_id
                   JOIN inventory i ON r.inventory_id = i.inventory_id
                   WHERE r.recipe_id = @id";

            return await _db.QueryFirstOrDefaultAsync<RecipeDisplayDTO>(sql, new { id });
        }

        public async Task<int> AddAsync(RecipeCreateDTO dto)
        {
            string sql = "INSERT INTO recipes (product_id, inventory_id, quantity_needed) VALUES (@ProductId, @InventoryId, @QuantityNeeded)";
            return await _db.ExecuteAsync(sql, dto);
        }

        public async Task<int> UpdateAsync(int id, RecipeUpdateDTO dto)
        {
            string sql = @"UPDATE recipes
                   SET product_id = @ProductId,
                       inventory_id = @InventoryId,
                       quantity_needed = @QuantityNeeded
                   WHERE recipe_id = @Id";

            return await _db.ExecuteAsync(sql, new
            {
                Id = id,
                dto.ProductId,
                dto.InventoryId,
                dto.QuantityNeeded
            });
        }

        public async Task<int> DeleteAsync(int id)
            => await _db.ExecuteAsync("DELETE FROM recipes WHERE recipe_id = @id", new { id });

        // --- TRIỂN KHAI CÁC HÀM MỞ RỘNG (JOIN) ---
        public async Task<IEnumerable<RecipeDisplayDTO>> GetAllDisplayAsync()
        {
            string sql = @"SELECT r.recipe_id, p.name as ProductName, i.name as InventoryName, 
                              r.quantity_needed, i.unit 
                       FROM recipes r
                       JOIN products p ON r.product_id = p.product_id
                       JOIN inventory i ON r.inventory_id = i.inventory_id";
            return await _db.QueryAsync<RecipeDisplayDTO>(sql);
        }

        public async Task<IEnumerable<RecipeDisplayDTO>> GetDisplayByProductIdAsync(int productId)
        {
            string sql = @"SELECT r.recipe_id, p.name as ProductName, i.name as InventoryName, 
                              r.quantity_needed, i.unit 
                       FROM recipes r
                       JOIN products p ON r.product_id = p.product_id
                       JOIN inventory i ON r.inventory_id = i.inventory_id
                       WHERE r.product_id = @productId";
            return await _db.QueryAsync<RecipeDisplayDTO>(sql, new { productId });
        }
    }
}

