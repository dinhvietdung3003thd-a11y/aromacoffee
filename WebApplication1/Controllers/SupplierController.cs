using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.DTOs.supplier;
using WebApplication1.Services.interfaces;

[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    public SupplierController(ISupplierService supplierService) => _supplierService = supplierService;

    // Trả về danh sách DTO rút gọn cho giao diện
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _supplierService.GetAllDisplayAsync();
        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _supplierService.GetByIdAsync(id));

    // Sử dụng SupplierDTO khi tạo mới để ẩn đi SupplierId
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupplierDTO dto)
    {
        var supplier = new Supplier
        {
            Name = dto.Name,
            ContactName = dto.ContactName,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address
        };

        var result = await _supplierService.AddAsync(supplier);
        return result > 0 ? Ok(new { message = "Thêm nhà cung cấp thành công" }) : BadRequest();
    }

    // Khi cập nhật, thường cần cả ID và thông tin mới
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupplierDTO dto)
    {
        var supplier = new Supplier
        {
            SupplierId = id,
            Name = dto.Name,
            ContactName = dto.ContactName,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address
        };

        var result = await _supplierService.UpdateAsync(supplier);
        return result > 0 ? Ok(new { message = "Cập nhật thành công" }) : BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _supplierService.DeleteAsync(id);
        return result > 0 ? Ok(new { message = "Xóa thành công" }) : NotFound();
    }
}