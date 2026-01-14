using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.categorys;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Services.interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICatagoryService _catagoryService;

        public CategoriesController(ICatagoryService catagoryService)
        {
            _catagoryService = catagoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _catagoryService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _catagoryService.GetByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            return Ok(await _catagoryService.SearchAsync(q));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CatagoryCreateDTO input)
        {
            var catagorydto = new CatagoryDTO
            {
                Description = input.Description,
                Name = input.Name,
            };

            await _catagoryService.AddAsync(catagorydto);
            return CreatedAtAction(nameof(GetById), new { id = catagorydto.Id }, catagorydto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _catagoryService  .DeleteAsync(id);
            return NoContent();
        }
    }
}