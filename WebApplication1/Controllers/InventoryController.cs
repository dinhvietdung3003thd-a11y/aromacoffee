using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.inventorys;
using WebApplication1.services.interfaces;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        public InventoryController(IInventoryService inventoryService) => _inventoryService = inventoryService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _inventoryService.GetAllAsync());

        [HttpPost("transaction")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateTransaction([FromBody] InventoryTransactionDTO dto)
        {
            try
            {
                var result = await _inventoryService.CreateTransactionAsync(dto);
                if (result) return Ok(new { message = "Xử lý giao dịch kho thành công!" });
                return BadRequest(new { message = "Không thể xử lý giao dịch kho." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi xử lý giao dịch kho.", detail = ex.Message });
            }
        }
        [HttpGet("report/summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int month, [FromQuery] int year)
        {
            var report = await _inventoryService.GetMonthlySummaryAsync(month, year);
            return Ok(report);
        }

        [HttpGet("report/supplier-spend")]
        public async Task<IActionResult> GetSupplierSpend([FromQuery] int month, [FromQuery] int year)
        {
            var report = await _inventoryService.GetSupplierSpendAsync(month, year);
            return Ok(report);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] InventoryCreateDTO dto)
        {
            try
            {
                var id = await _inventoryService.CreateAsync(dto);
                return Ok(new
                {
                    message = "Tạo nguyên liệu thành công!",
                    inventoryId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
