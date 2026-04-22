namespace WebApplication1.Models
{
    public class InventoryTransaction
    {
        public int TransactionId { get; set; } 
        public int InventoryId { get; set; } 

        // Loại giao dịch: 'Import' hoặc 'Export'
        public string TransactionType { get; set; } = "Import";

        public decimal Quantity { get; set; } 
        public DateTime TransactionDate { get; set; } = DateTime.Now; 
        public int UserId { get; set; } // ID nhân viên thực hiện
        public string? Note { get; set; }
        public decimal Price { get; set; } 
    }
}