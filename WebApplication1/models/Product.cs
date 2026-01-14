namespace WebApplication1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? CategoryId { get; set; } // Khóa ngoại (FK)
        public string? ImageUrl { get; set; }
        public bool Status { get; set; } // bit(1) -> bool
    }
}