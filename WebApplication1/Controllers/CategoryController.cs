using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.categorys;
using WebApplication1.Models;
using WebApplication1.services;
using WebApplication1.services.interfaces;

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
            return Ok(await _categoryService.GetAllDisplayAsync());
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
        public async Task<IActionResult> Create([FromBody] CategoryCreateDTO dto)
        {
            int newid = await _categoryService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newid }, new {CategoryId = newid});
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDTO dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            return result > 0
                ? Ok(new { message = "Cập nhật thành công" })
                : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService  .DeleteAsync(id);
            return NoContent();
        }
    }
}