using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.services.interfaces;

[ApiController]
[Route("api/[controller]")]
public class InventoryTransactionController : ControllerBase
{
    private readonly IInventoryTransactionService _service;
    public InventoryTransactionController(IInventoryTransactionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllDisplayAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InventoryTransaction transaction)
    {
        var result = await _service.AddAsync(transaction);
        return result > 0 ? Ok(new { message = "Ghi nhật ký thành công" }) : BadRequest();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q) => Ok(await _service.SearchAsync(q));
}