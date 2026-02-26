namespace WebApplication1.DTOs.order
{
    public class OrderDisplayDTO
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? TableId { get; set; }
        public string? Status { get; set; }
        public string? CreatorFullName { get; set; }
        public int? UserId { get; set; }
        public string? ShippingAddress { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public double? DistanceKm { get; set; }
        public decimal? ShippingFee { get; set; }
        public List<OrderDetailDTO> Details { get; set; } = new List<OrderDetailDTO>();
    }
}