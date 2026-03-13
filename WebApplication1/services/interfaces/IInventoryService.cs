using WebApplication1.DTOs.inventorys;

namespace WebApplication1.services.interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDisplayDTO>> GetAllAsync();
        Task<InventoryDisplayDTO?> GetByIdAsync(int id);
        // Hàm quan trọng nhất để xử lý nhập/xuất kho
        Task<bool> CreateTransactionAsync(InventoryTransactionDTO dto);
        Task<IEnumerable<InventorySummaryReportDTO>> GetMonthlySummaryAsync(int month, int year);
        Task<IEnumerable<SupplierSpendReportDTO>> GetSupplierSpendAsync(int month, int year);
        Task<int> CreateAsync(InventoryCreateDTO dto);

    }
}

