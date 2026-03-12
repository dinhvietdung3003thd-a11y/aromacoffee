using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.recipe;
using WebApplication1.services.interfaces;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpGet("display-all")]
        public async Task<IActionResult> GetAllDisplay()
        {
            return Ok(await _recipeService.GetAllDisplayAsync());
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            return Ok(await _recipeService.GetDisplayByProductIdAsync(productId));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecipeCreateDTO dto)
        {
            var result = await _recipeService.AddAsync(dto);
            return result > 0 ? Ok(new { message = "Thêm thành công" }) : BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _recipeService.DeleteAsync(id);
            return result > 0 ? Ok(new { message = "Xóa thành công" }) : NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            return recipe == null ? NotFound() : Ok(recipe);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RecipeUpdateDTO dto)
        {
            var result = await _recipeService.UpdateAsync(id, dto);
            return result > 0 ? Ok(new { message = "Cập nhật thành công" }) : NotFound();
        }
    }
}