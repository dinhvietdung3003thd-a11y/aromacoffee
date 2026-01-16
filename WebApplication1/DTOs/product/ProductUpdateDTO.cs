namespace WebApplication1.DTOs.product
{
    public class ProductUpdateDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        public int CategoryId { get; set; }
    }
}
