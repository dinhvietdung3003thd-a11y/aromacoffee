using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.DTOs.order;
using WebApplication1.services.interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/client/orders")]
    [Authorize(Roles = "Customer")]
    public class ClientOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public ClientOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(customerIdClaim))
            {
                return Unauthorized(new { message = "Không xác định được khách hàng." });
            }

            if (!int.TryParse(customerIdClaim, out int customerId))
            {
                return Unauthorized(new { message = "ID khách hàng không hợp lệ." });
            }

            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }
            
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerCreateOrderDTO input)
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new { message = "Không lấy được customerId từ token" });

            if (!int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new { message = "Định danh khách hàng trong token không hợp lệ." });

            try
            {
                int newId = await _orderService.AddByCustomerAsync(input, customerId);
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

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new { message = "Không xác định được khách hàng." });

            if (!int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new { message = "Định danh khách hàng trong token không hợp lệ." });

            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (order.CustomerId != customerId)
                return StatusCode(403, new { message = "Bạn không có quyền xem đơn hàng này." });

            return Ok(order);
        }
    }
}
