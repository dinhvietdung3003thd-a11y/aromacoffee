using Dapper;
using System.Data;
using WebApplication1.DTOs.recipe;
using WebApplication1.DTOs.recipes;
using WebApplication1.Models;
using WebApplication1.Services.interfaces;

public class RecipeService : IRecipeService
{
    private readonly IDbConnection _db;
    public RecipeService(IDbConnection db) => _db = db;

    // --- TRIỂN KHAI CÁC HÀM TỪ IBASE ---
    public async Task<IEnumerable<Recipe>> GetAllAsync()
        => await _db.QueryAsync<Recipe>("SELECT * FROM recipes");

    public async Task<Recipe?> GetByIdAsync(int id)
        => await _db.QueryFirstOrDefaultAsync<Recipe>("SELECT * FROM recipes WHERE recipe_id = @id", new { id });

    public async Task<int> AddAsync(Recipe entity)
    {
        string sql = "INSERT INTO recipes (product_id, inventory_id, quantity_needed) VALUES (@ProductId, @InventoryId, @QuantityNeeded)";
        return await _db.ExecuteAsync(sql, entity);
    }

    public async Task<int> UpdateAsync(Recipe entity)
    {
        string sql = "UPDATE recipes SET product_id = @ProductId, inventory_id = @InventoryId, quantity_needed = @QuantityNeeded WHERE recipe_id = @RecipeId";
        return await _db.ExecuteAsync(sql, entity);
    }

    public async Task<int> DeleteAsync(int id)
        => await _db.ExecuteAsync("DELETE FROM recipes WHERE recipe_id = @id", new { id });

    public async Task<IEnumerable<Recipe>> SearchAsync(string keyword)
    {
        // Tìm kiếm công thức dựa trên ID sản phẩm hoặc ID nguyên liệu
        string sql = "SELECT * FROM recipes WHERE product_id LIKE @k OR inventory_id LIKE @k";
        return await _db.QueryAsync<Recipe>(sql, new { k = $"%{keyword}%" });
    }

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