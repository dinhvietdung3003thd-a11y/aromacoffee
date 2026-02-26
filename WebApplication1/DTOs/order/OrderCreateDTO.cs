namespace WebApplication1.DTOs.order
{
    public class OrderCreateDTO
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal? TotalAmount { get; set; }
        public int? TableId { get; set; }
        public string? Status { get; set; }
        public int? UserId { get; set; } // Username người tạo
        public int? CustomerId { get; set; }
        public string? Note { get; set; }
        public string? ShippingAddress { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal ShippingFee { get; set; }
        public List<OrderDetailCreateDTO> Details { get; set; } = new();
    }
}