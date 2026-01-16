namespace WebApplication1.DTOs.order
{
    public class OrderDetailCreateDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Giá lúc khách bấm mua
    }
}