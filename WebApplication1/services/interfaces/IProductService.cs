using WebApplication1.DTOs.product;
using WebApplication1.Models;
using WebApplication1.services.interfaces;
namespace WebApplication1.services.interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync(); // Có chữ Async
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(ProductCreateDTO dto);
        Task<int> UpdateAsync(int id, ProductUpdateDTO dto);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<ProductDTO>> SearchAsync(string keyword);
        Task<IEnumerable<ProductDTO>> SearchProductsElasticAsync(string keyword);
        Task SyncProductsToElasticAsync();
        Task<IEnumerable<ProductIngredientAvailabilityDTO>> GetIngredientAvailabilityAsync(int? productId = null);
    }
}

