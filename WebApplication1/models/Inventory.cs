namespace WebApplication1.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; } 
        public string Name { get; set; } = string.Empty; 
        public string Unit { get; set; } = string.Empty; 
        public decimal QuantityInStock { get; set; } 
        public decimal MinThreshold { get; set; } 
        public DateTime UpdatedAt { get; set; } = DateTime.Now; 
    }
}