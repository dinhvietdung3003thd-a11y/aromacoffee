namespace WebApplication1.DTOs.product
{
    public class ProductCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool Status { get; set; }

        public int CategoryId { get; set; }
    }
}
