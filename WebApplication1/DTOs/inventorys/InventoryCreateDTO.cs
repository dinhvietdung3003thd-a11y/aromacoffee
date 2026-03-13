namespace WebApplication1.DTOs.inventorys
{
    public class InventoryCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal QuantityInStock { get; set; } = 0;
        public decimal MinThreshold { get; set; }
        public int? SupplierId { get; set; }
    }
}