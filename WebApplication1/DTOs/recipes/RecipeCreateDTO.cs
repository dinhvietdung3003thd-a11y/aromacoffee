namespace WebApplication1.DTOs.recipe
{
    public class RecipeCreateDTO
    {
        public int ProductId { get; set; }
        public int InventoryId { get; set; }
        public decimal QuantityNeeded { get; set; }
    }
}