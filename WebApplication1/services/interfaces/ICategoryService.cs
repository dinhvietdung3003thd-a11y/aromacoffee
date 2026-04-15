using WebApplication1.DTOs.categorys;

namespace WebApplication1.services.interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllDisplayAsync();
        Task<CategoryDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(CategoryCreateDTO dto);
        Task<int> UpdateAsync(int id, CategoryDTO dto);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<CategoryDTO>> SearchAsync(string keyword);
    }
}