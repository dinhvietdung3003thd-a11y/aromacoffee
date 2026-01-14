namespace WebApplication1.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? CreatedBy { get; set; } // Username người tạo
        public int? TableNumber { get; set; }
        public string? Status { get; set; }
    }
}