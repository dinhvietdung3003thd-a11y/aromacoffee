namespace WebApplication1.DTOs.report
{
    public class InventorySummaryReportDTO
    {
        public int InventoryId { get; set; } 
        public string InventoryName { get; set; } = string.Empty; 
        public decimal OpeningStock { get; set; } // Tồng đầu kỳ
        public decimal TotalImport { get; set; } // Tổng nhập
        public decimal TotalExport { get; set; } // Tổng xuất
        public decimal ClosingStock { get; set; } // Tồn cuối kỳ
        public string Unit { get; set; } = string.Empty; 
    }
}