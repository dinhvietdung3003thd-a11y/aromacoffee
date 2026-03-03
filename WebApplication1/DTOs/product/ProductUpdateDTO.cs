using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.product
{
    public class ProductUpdateDTO
    {
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price phải >= 0")]
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId không hợp lệ")]
        public int CategoryId { get; set; }
    }
}
