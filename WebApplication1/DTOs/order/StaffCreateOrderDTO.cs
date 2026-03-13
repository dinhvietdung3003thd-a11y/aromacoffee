namespace WebApplication1.DTOs.order
{
    public class StaffCreateOrderDTO
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int? TableId { get; set; }
        public string? Status { get; set; } = "Pending";
        public int? CustomerId { get; set; }
        public string? Note { get; set; }
        public List<OrderDetailCreateDTO> Details { get; set; } = new();
    }
}