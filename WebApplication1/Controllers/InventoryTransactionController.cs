using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.services.interfaces;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IInventoryTransactionService _service;
        public InventoryTransactionController(IInventoryTransactionService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllDisplayAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q) => Ok(await _service.SearchAsync(q));
    }
}