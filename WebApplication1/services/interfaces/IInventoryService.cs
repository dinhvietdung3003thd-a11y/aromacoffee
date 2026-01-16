using WebApplication1.DTOs.inventory;
using WebApplication1.DTOs.report;

public interface IInventoryService
{
    Task<IEnumerable<InventoryDisplayDTO>> GetAllAsync();
    Task<InventoryDisplayDTO?> GetByIdAsync(int id);
    // Hàm quan trọng nhất để xử lý nhập/xuất kho
    Task<bool> ProcessTransactionAsync(InventoryTransactionDTO dto);
    Task<IEnumerable<InventorySummaryReportDTO>> GetMonthlySummaryAsync(int month, int year);
    Task<IEnumerable<SupplierSpendReportDTO>> GetSupplierSpendAsync(int month, int year);

}