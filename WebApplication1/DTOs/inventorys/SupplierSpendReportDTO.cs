namespace WebApplication1.DTOs.report
{
    public class SupplierSpendReportDTO
    {
        public string SupplierName { get; set; } = string.Empty; 
        public decimal TotalSpent { get; set; } // Tổng tiền đã chi
        public int TransactionCount { get; set; } // Số lần nhập hàng
    }
}