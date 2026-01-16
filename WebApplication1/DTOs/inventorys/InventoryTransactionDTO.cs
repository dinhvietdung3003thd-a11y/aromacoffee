namespace WebApplication1.DTOs.inventory
{
    public class InventoryTransactionDTO
    {
        public int InventoryId { get; set; } 
        public string TransactionType { get; set; } = "Import"; // 'Import' hoặc 'Export'
        public decimal Quantity { get; set; } 
        public int UserId { get; set; } // Người thực hiện
        public string? Note { get; set; } 
    }
}