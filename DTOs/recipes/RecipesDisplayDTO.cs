namespace WebApplication1.DTOs.recipes
{
    public class RecipeDisplayDTO
    {
        public int RecipeId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Tên món ăn
        public string InventoryName { get; set; } = string.Empty; // Tên nguyên liệu
        public decimal QuantityNeeded { get; set; }
        public string Unit { get; set; } = string.Empty; // Đơn vị tính (kg, ml...)
    }
}