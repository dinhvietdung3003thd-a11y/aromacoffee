namespace WebApplication1.DTOs.inventory
{
    public class InventoryDisplayDTO
    {
        public int IngredientId { get; set; } 
        public string Name { get; set; } = string.Empty; 
        public decimal QuantityInStock { get; set; } 
        public string Unit { get; set; } = string.Empty; 
        public decimal MinThreshold { get; set; } 
        public bool IsLowStock => QuantityInStock <= MinThreshold;
    }
}