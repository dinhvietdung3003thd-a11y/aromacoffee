using WebApplication1.DTOs;
using WebApplication1.services.interfaces;

namespace WebApplication1.services.interfaces
{
    public interface ITableService : IBaseService<TableDTO>
    {
        // Hàm cập nhật trạng thái bàn (Trống <-> Có người) theo quy trình gọi món
        Task<int> UpdateStatusAsync(int id, string status);
    }
}