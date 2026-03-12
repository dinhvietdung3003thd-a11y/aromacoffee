
using WebApplication1.DTOs.tablefood;
namespace WebApplication1.services.interfaces
{
    public interface ITableService
    {
        Task<IEnumerable<TableDTO>> GetAllDisplayAsync();
        Task<TableDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(TableCreateDTO dto);
        Task<int> UpdateAsync(int id, TableDTO dto);
        Task<int> DeleteAsync(int id);
        Task<int> UpdateStatusAsync(int id, string status);
    }
}
