using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Common;
using WebApplication1.DTOs.order;
using WebApplication1.services.interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
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

        // 3. Tìm kiếm đơn hàng
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
        public async Task<IActionResult> Create([FromBody] StaffCreateOrderDTO input)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Không lấy được userId từ token" });

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Định danh người dùng trong token không hợp lệ." });

            try
            {
                int newId = await _orderService.AddByStaffAsync(input, userId);
                return CreatedAtAction(nameof(GetById), new { id = newId }, new { OrderId = newId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // 6. Cập nhật thông tin đơn hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateDTO input)
        {
            input.OrderId = id;

            try
            {
                var result = await _orderService.UpdateAsync(input);
                return result.Updated
                    ? Ok(new { message = result.Message, warnings = result.Warnings })
                    : NotFound(new { message = "Không tìm thấy đơn hàng." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật đơn hàng." });
            }
        }

        // 7. Cập nhật riêng trạng thái (Ví dụ: Chuyển sang "Đã thanh toán")
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { message = "Trạng thái không được để trống." });

            var normalizedStatus = status.Trim();
            if (!StatusConstants.OrderStatuses.Contains(normalizedStatus))
                return BadRequest(new { message = "Trạng thái đơn hàng không hợp lệ." });

            try
            {
                var result = await _orderService.UpdateStatusAsync(id, normalizedStatus);
                if (!result.Updated)
                    return NotFound(new { message = "Không thể cập nhật trạng thái đơn hàng." });

                return Ok(new { message = result.Message, warnings = result.Warnings });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái đơn hàng." });
            }
        }

        // 8. Xóa đơn hàng
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _orderService.DeleteAsync(id);
            return rows > 0 ? Ok(new { message = "Đã xóa đơn hàng" }) : NotFound(new { message = "Không tìm thấy đơn hàng." });
        }
    }
}
