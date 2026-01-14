using WebApplication1.DTOs.order;
using WebApplication1.Services.interfaces;

namespace WebApplication1.services.interfaces
{
    // Kế thừa IBaseService với kiểu dữ liệu là OrderDisplayDTO
    public interface IOrderService : IBaseService<OrderDisplayDTO>
    {
        // Ngoài các hàm CRUD cơ bản, Order thường cần thêm hàm cập nhật trạng thái
        // Ví dụ: Chuyển từ "Chờ thanh toán" sang "Đã thanh toán" hoặc "Đã hủy"
        Task<int> UpdateStatusAsync(int id, string status);

        // Bạn có thể thêm hàm lấy đơn hàng theo bàn nếu cần
        Task<IEnumerable<OrderDisplayDTO>> GetOrdersByTableAsync(int tableNumber);
    }
}