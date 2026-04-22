using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.order
{
    public class OrderDetailCreateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId phải > 0")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity phải > 0")]
        public int Quantity { get; set; }
    }
}