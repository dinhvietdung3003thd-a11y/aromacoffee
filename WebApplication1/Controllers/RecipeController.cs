using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.interfaces;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    public RecipeController(IRecipeService recipeService) => _recipeService = recipeService;

    [HttpGet("display-all")]
    public async Task<IActionResult> GetAllDisplay() => Ok(await _recipeService.GetAllDisplayAsync());

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
        => Ok(await _recipeService.GetDisplayByProductIdAsync(productId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Recipe recipe)
    {
        var result = await _recipeService.AddAsync(recipe);
        return result > 0 ? Ok(new { message = "Thêm thành công" }) : BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _recipeService.DeleteAsync(id);
        return result > 0 ? Ok(new { message = "Xóa thành công" }) : NotFound();
    }
}