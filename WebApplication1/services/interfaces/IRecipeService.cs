using WebApplication1.DTOs.recipe;
using WebApplication1.DTOs.recipes;

namespace WebApplication1.services.interfaces
{
    public interface IRecipeService
    {
        Task<IEnumerable<RecipeDisplayDTO>> GetAllDisplayAsync();
        Task<RecipeDisplayDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(RecipeCreateDTO dto);
        Task<int> UpdateAsync(int id, RecipeUpdateDTO dto);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<RecipeDisplayDTO>> GetDisplayByProductIdAsync(int productId);
    }
}