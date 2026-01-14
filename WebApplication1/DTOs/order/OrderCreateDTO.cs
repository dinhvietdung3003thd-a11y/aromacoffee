namespace WebApplication1.DTOs.order
{
    public class OrderCreateDTO
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal? TotalAmount { get; set; }
        public int? TableNumber { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; } // Username người tạo
    }
}