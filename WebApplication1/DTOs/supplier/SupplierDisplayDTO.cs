namespace WebApplication1.DTOs.supplier
{
    public class SupplierDisplayDTO
    {
        public int SupplierId { get; set; } 
        public string Name { get; set; } = string.Empty; 
        public string? Phone { get; set; } 
        public string? ContactPerson { get; set; } 
        public int TotalProductsSupplied { get; set; }
    }
}