using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.order
{
    public class OrderCreateDTO
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue, ErrorMessage = "TableId không hợp lệ")]
        public int? TableId { get; set; }

        [RegularExpression("Pending|Paid|Completed|Cancelled", ErrorMessage = "Status không hợp lệ")]
        public string? Status { get; set; }

        public int? UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CustomerId không hợp lệ")]
        public int? CustomerId { get; set; }

        [StringLength(500, ErrorMessage = "Note tối đa 500 ký tự")]
        public string? Note { get; set; }

        [MinLength(1, ErrorMessage = "Order phải có ít nhất 1 món")]
        public List<OrderDetailCreateDTO> Details { get; set; } = new();

        //[MinLength(3)]
        //public string? ShippingAddress { get; set; }

        //[Range(-90, 90, ErrorMessage = "Lat không hợp lệ")]
        //public double? Lat { get; set; }

        //[Range(-180, 180, ErrorMessage = "Lng không hợp lệ")]
        //public double? Lng { get; set; }

        //[Range(0, double.MaxValue, ErrorMessage = "DistanceKm phải >= 0")]
        //public decimal? DistanceKm { get; set; }
    }
}