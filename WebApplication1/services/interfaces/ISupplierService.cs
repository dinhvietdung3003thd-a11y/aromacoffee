using WebApplication1.DTOs.supplier;
using WebApplication1.Models;
using WebApplication1.Services.interfaces;

namespace WebApplication1.Services.interfaces
{
    public interface ISupplierService : IBaseService<Supplier>
    {
        Task<IEnumerable<SupplierDisplayDTO>> GetAllDisplayAsync();
    }
}