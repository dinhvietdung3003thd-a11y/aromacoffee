namespace WebApplication1.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; } // Khóa chính
        public int ProductId { get; set; } 
        public int InventoryId { get; set; }    
        public decimal QuantityNeeded { get; set; } // Lượng nguyên liệu cần
    }
}