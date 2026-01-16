using WebApplication1.DTOs.inventory;

public interface IInventoryService
{
    Task<IEnumerable<InventoryDisplayDTO>> GetAllAsync();
    Task<InventoryDisplayDTO?> GetByIdAsync(int id);
    // Hàm quan trọng nhất để xử lý nhập/xuất kho
    Task<bool> ProcessTransactionAsync(InventoryTransactionDTO dto);
}