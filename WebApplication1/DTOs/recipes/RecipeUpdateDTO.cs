namespace WebApplication1.DTOs.recipes
{
    public class RecipeUpdateDTO
    {
        public int RecipeId { get; set; }
        public int ProductId { get; set; }
        public int InventoryId { get; set; }
        public decimal QuantityNeeded { get; set; }
    }
}