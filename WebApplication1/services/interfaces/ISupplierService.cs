using WebApplication1.DTOs.supplier;

namespace WebApplication1.services.interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDisplayDTO>> GetAllDisplayAsync();
        Task<SupplierDisplayDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(SupplierDTO dto);
        Task<int> UpdateAsync(int id, SupplierDTO dto);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<SupplierDisplayDTO>> SearchAsync(string keyword);
    }
}