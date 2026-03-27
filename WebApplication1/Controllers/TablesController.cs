using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.tablefood;
using WebApplication1.Common;
using WebApplication1.services.interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;
        public TablesController(ITableService tableService) => _tableService = tableService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _tableService.GetAllDisplayAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            return table == null ? NotFound() : Ok(table);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TableCreateDTO tableDto)
        {
            await _tableService.AddAsync(tableDto);
            return Ok(new { message = "Thêm bàn thành công" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { message = "Trạng thái bàn không được để trống." });

            var normalizedStatus = status.Trim();
            if (!StatusConstants.TableStatuses.Contains(normalizedStatus))
                return BadRequest(new { message = "Trạng thái bàn không hợp lệ." });

            var rows = await _tableService.UpdateStatusAsync(id, normalizedStatus);
            if (rows == 0)
                return NotFound(new { message = "Không tìm thấy bàn." });

            return Ok(new { message = "Cập nhật trạng thái bàn thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _tableService.DeleteAsync(id);
            return rows > 0 ? Ok(new { message = "Đã xóa bàn" }) : NotFound(new { message = "Không tìm thấy bàn." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TableDTO dto)
        {
            if (id != dto.TableId)
                return BadRequest(new { message = "Mã bàn không khớp!" });

            var rows = await _tableService.UpdateAsync(id, dto);

            if (rows == 0)
                return NotFound(new { message = "Không tìm thấy bàn." });

            return Ok(new { message = "Cập nhật bàn thành công" });
        }
    }
}
