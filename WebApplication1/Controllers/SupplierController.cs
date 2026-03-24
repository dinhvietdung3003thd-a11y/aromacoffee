using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.supplier;
using WebApplication1.services.interfaces;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _supplierService.GetAllDisplayAsync();
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return Ok(supplier);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] SupplierDTO dto)
        {
            var result = await _supplierService.AddAsync(dto);
            return result > 0
                ? Ok(new { message = "Thêm nhà cung cấp thành công" })
                : BadRequest(new { message = "Không thể thêm nhà cung cấp." });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] SupplierDTO dto)
        {
            var result = await _supplierService.UpdateAsync(id, dto);
            return result > 0
                ? Ok(new { message = "Cập nhật thành công" })
                : NotFound(new { message = "Không tìm thấy nhà cung cấp." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _supplierService.DeleteAsync(id);
            return result > 0
                ? Ok(new { message = "Xóa thành công" })
                : NotFound(new { message = "Không tìm thấy nhà cung cấp." });
        }
    }
}
