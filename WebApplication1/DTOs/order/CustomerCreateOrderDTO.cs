using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.order
{
    public class CustomerCreateOrderDTO
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue, ErrorMessage = "TableId không hợp lệ")]
        public int? TableId { get; set; }

        [StringLength(500, ErrorMessage = "Note tối đa 500 ký tự")]
        public string? Note { get; set; }

        [MinLength(1, ErrorMessage = "Order phải có ít nhất 1 món")]
        public List<OrderDetailCreateDTO> Details { get; set; } = new();
    }
}