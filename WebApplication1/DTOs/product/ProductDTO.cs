namespace WebApplication1.DTOs.product
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        public int CategoryId { get; set; }
        // Đây là thuộc tính lấy từ bảng categories sau khi JOIN
        public string CategoryName { get; set; } = string.Empty;
    }
}