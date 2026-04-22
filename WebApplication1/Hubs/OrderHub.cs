using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Hubs
{
    // Hub là nơi quản lý các kết nối giữa Server và Client
    public class OrderHub : Hub
    {
        // Hàm này dùng để Shipper gửi tọa độ cho khách xem
        public async Task SendLocation(int orderId, double lat, double lng)
        {
            // Gửi tới một "Nhóm" (Group) những người đang quan tâm đến đơn hàng này
            await Clients.Group(orderId.ToString()).SendAsync("ReceiveLocation", lat, lng);
        }

        // Khi khách hàng mở trang theo dõi đơn, họ sẽ "vào phòng" (Join Group) của đơn đó
        public async Task JoinOrderTracking(int orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId.ToString());
        }
    }
}