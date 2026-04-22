using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.recipes
{
    public class RecipeCreateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId phải > 0")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "InventoryId phải > 0")]
        public int InventoryId { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "QuantityNeeded phải > 0")]
        public decimal QuantityNeeded { get; set; }
    }
}