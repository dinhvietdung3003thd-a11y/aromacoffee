namespace WebApplication1.DTOs.order
{
    public class OrderCreateDTO
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal? TotalAmount { get; set; }
        public int? TableId { get; set; }
        public string? Status { get; set; }
        public int? UserId { get; set; } // Username người tạo
        public List<OrderDetailCreateDTO> Details { get; set; } = new();
    }
}