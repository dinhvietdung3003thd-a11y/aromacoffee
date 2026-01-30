using WebApplication1.Models;
using WebApplication1.DTOs.inventory;

namespace WebApplication1.services.interfaces
{
    public interface IInventoryTransactionService
    {
        // Chỉ cho phép lấy dữ liệu và thêm mới, không có Update/Delete
        Task<IEnumerable<InventoryTransaction>> GetAllAsync();
        Task<InventoryTransaction?> GetByIdAsync(int id);
        Task<int> AddAsync(InventoryTransaction entity);
        Task<IEnumerable<InventoryTransaction>> SearchAsync(string keyword);

        // Hàm mở rộng để hiển thị tên nguyên liệu và nhân viên
        Task<IEnumerable<InventoryTransactionDisplayDTO>> GetAllDisplayAsync();
    }
}