using WebApplication1.DTOs.recipe;
using WebApplication1.DTOs.recipes;
using WebApplication1.Models;
namespace WebApplication1.services.interfaces
{
    public interface IRecipeService : IBaseService<Recipe>
    {
        // Hàm bổ sung để lấy dữ liệu kèm theo tên (JOIN 3 bảng)
        Task<IEnumerable<RecipeDisplayDTO>> GetAllDisplayAsync();
        Task<IEnumerable<RecipeDisplayDTO>> GetDisplayByProductIdAsync(int productId);
    }
}