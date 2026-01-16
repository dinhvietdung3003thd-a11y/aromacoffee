namespace WebApplication1.DTOs.order
{
    public class OrderDetailDTO
    {
        public int OrderDetailId { get; set; } 
        public int ProductId { get; set; } 
        public string ProductName { get; set; } = string.Empty; // Lấy từ bảng products
        public int Quantity { get; set; } 
        public decimal UnitPrice { get; set; } 
        public decimal Subtotal { get; set; } 
    }
}