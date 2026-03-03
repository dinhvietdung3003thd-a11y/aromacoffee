using System.ComponentModel.DataAnnotations;

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

        [MinLength(3)]
        public string? ShippingAddress { get; set; }

        [Range(-90, 90, ErrorMessage = "Lat không hợp lệ")]
        public double? Lat { get; set; }

        [Range(-180, 180, ErrorMessage = "Lng không hợp lệ")]
        public double? Lng { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "DistanceKm phải >= 0")]
        public decimal? DistanceKm { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "ShippingFee phải >= 0")]
        public decimal ShippingFee { get; set; }
        public List<OrderDetailCreateDTO> Details { get; set; } = new();
    }
}