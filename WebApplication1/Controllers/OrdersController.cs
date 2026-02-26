using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.order;
using WebApplication1.services.interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 1. Lấy danh sách tất cả đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        // 2. Lấy chi tiết đơn hàng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            return Ok(order);
        }

        // 3. Tìm kiếm đơn hàng (theo trạng thái hoặc số bàn)
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var results = await _orderService.SearchAsync(q);
            return Ok(results);
        }

        // 4. Lấy đơn hàng theo số bàn
        [HttpGet("table/{tableNumber}")]
        public async Task<IActionResult> GetByTable(int tableNumber)
        {
            var orders = await _orderService.GetOrdersByTableAsync(tableNumber);
            return Ok(orders);
        }

        // 5. Tạo mới đơn hàng
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDTO input)
        {
            int newId = await _orderService.AddAsync(input);

            return CreatedAtAction(nameof(GetById), new { id = newId }, input);
        }

        // 6. Cập nhật thông tin đơn hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateDTO input)
        {
            // Kiểm tra an toàn: ID trên đường dẫn phải khớp với dữ liệu gửi lên
            // Gán id từ URL vào thuộc tính OrderId của DTO để Service sử dụng
            input.OrderId = id;

            try
            {
                // Service sẽ xóa các món cũ và chèn lại các món mới trong Details
                await _orderService.UpdateAsync(input);
                return Ok(new { message = "Cập nhật đơn hàng và chi tiết món thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        // 7. Cập nhật riêng trạng thái (Ví dụ: Chuyển sang "Đã thanh toán")
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            await _orderService.UpdateStatusAsync(id, status);
            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }

        // 8. Xóa đơn hàng
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return Ok(new { message = "Đã xóa đơn hàng" });
        }
    }
}