namespace WebApplication1.DTOs.order
{
    public class CustomerCreateOrderDTO
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int? TableId { get; set; }
        public string? Status { get; set; } = "Pending";
        public string? Note { get; set; }
        public List<OrderDetailCreateDTO> Details { get; set; } = new();
    }
}