namespace WebApplication1.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int userId { get; set; } // ID người tạo
        public int? TableId { get; set; }
        public string? Status { get; set; }
        public int? CustomerId { get; set; }
        public string? Note { get; set; }
    }
}