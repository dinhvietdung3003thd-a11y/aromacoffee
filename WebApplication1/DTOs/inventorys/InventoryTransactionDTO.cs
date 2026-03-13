using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.inventorys
{
    public class InventoryTransactionDTO
    {
        [Required]
        public int InventoryId { get; set; }

        [Required]
        public string TransactionType { get; set; } = "Import"; // 'Import' hoặc 'Export'

        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity phải > 0")]
        public decimal Quantity { get; set; }

        //sau khi them authorize thi loai bo userId va lay tu token
        public int UserId { get; set; } // Người thực hiện
        public string? Note { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price phải >= 0")]
        public decimal Price { get; set; }
    }
}