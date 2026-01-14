using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.services.interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;
        public TablesController(ITableService tableService) => _tableService = tableService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _tableService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            return table == null ? NotFound() : Ok(table);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TableDTO tableDto)
        {
            await _tableService.AddAsync(tableDto);
            return Ok(new { message = "Thêm bàn thành công" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            await _tableService.UpdateStatusAsync(id, status);
            return Ok(new { message = "Cập nhật trạng thái bàn thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _tableService.DeleteAsync(id);
            return Ok(new { message = "Đã xóa bàn" });
        }
    }
}