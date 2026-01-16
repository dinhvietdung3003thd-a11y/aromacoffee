namespace WebApplication1.DTOs.inventory
{
    public class InventoryDisplayDTO
    {
        public int InventoryId { get; set; } 
        public string Name { get; set; } = string.Empty; 
        public decimal QuantityInStock { get; set; } 
        public string Unit { get; set; } = string.Empty; 
        public decimal MinThreshold { get; set; }
        public string SupplierName { get; set; } = string .Empty;
        public bool IsLowStock => QuantityInStock <= MinThreshold;
    }
}