using WebApplication1.DTOs.supplier;
using WebApplication1.Models;
using WebApplication1.services.interfaces;

namespace WebApplication1.services.interfaces
{
    public interface ISupplierService : IBaseService<Supplier>
    {
        Task<IEnumerable<SupplierDisplayDTO>> GetAllDisplayAsync();
    }
}