using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.inventory;
using WebApplication1.services.interfaces;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    public InventoryController(IInventoryService inventoryService) => _inventoryService = inventoryService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _inventoryService.GetAllAsync());

    [HttpPost("transaction")]
    public async Task<IActionResult> CreateTransaction([FromBody] InventoryTransactionDTO dto)
    {
        var result = await _inventoryService.ProcessTransactionAsync(dto);
        if (result) return Ok(new { message = "Xử lý giao dịch kho thành công!" });
        return BadRequest(new { message = "Lỗi khi xử lý giao dịch kho!" });
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
}