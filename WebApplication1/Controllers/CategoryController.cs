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
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _categoryService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            return Ok(await _categoryService.SearchAsync(q));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDTO input)
        {
            var categorydto = new CategoryDTO
            {
                Description = input.Description,
                Name = input.Name,
            };

            await _categoryService.AddAsync(categorydto);
            return CreatedAtAction(nameof(GetById), new { id = categorydto.Id }, categorydto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService  .DeleteAsync(id);
            return NoContent();
        }
    }
}