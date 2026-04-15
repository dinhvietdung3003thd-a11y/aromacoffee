using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.inventorys
{
    public class InventoryTransactionDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "InventoryId phải > 0")]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "TransactionType không được để trống")]
        [RegularExpression("Import|Export", ErrorMessage = "TransactionType phải là Import hoặc Export")]
        public string TransactionType { get; set; } = "Import";

        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity phải > 0")]
        public decimal Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Note tối đa 500 ký tự")]
        public string? Note { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price phải >= 0")]
        public decimal Price { get; set; }
    }
}