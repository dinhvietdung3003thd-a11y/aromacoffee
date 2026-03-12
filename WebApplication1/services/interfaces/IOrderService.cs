using WebApplication1.DTOs.order;

namespace WebApplication1.services.interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDisplayDTO>> GetAllAsync();
        Task<OrderDisplayDTO?> GetByIdAsync(int id);
        Task<IEnumerable<OrderDisplayDTO>> GetOrdersByTableAsync(int tableNumber);
        Task<int> AddByStaffAsync(StaffCreateOrderDTO dto, int userId);
        Task<int> AddByCustomerAsync(CustomerCreateOrderDTO dto, int customerId);
        Task<int> UpdateAsync(OrderUpdateDTO dto);
        Task<int> UpdateStatusAsync(int id, string status);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<OrderDisplayDTO>> SearchAsync(string key);
    }
}