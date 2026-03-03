using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.order
{
    public class OrderDetailCreateDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity phải >= 1")]
        public int Quantity { get; set; }
    }
}