namespace WebApplication1.DTOs.inventory
{
    public class InventoryTransactionDisplayDTO
    {
        public int TransactionId { get; set; }
        public string InventoryName { get; set; } = string.Empty; // Lấy từ bảng inventory
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty; // Đơn vị tính (kg, lít...)
        public DateTime TransactionDate { get; set; }
        public string StaffName { get; set; } = string.Empty; // Lấy từ bảng users (full_name)
        public string? Note { get; set; }
    }
}
