using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.product;
using WebApplication1.services.interfaces;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _productService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDTO input)
        {
            var id = await _productService.AddAsync(input);
            return CreatedAtAction(nameof(GetById), new { id }, new { ProductId = id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductUpdateDTO input)
        {
            if (id != input.ProductId)
                return BadRequest("Mã sản phẩm không khớp!");

            var rows = await _productService.UpdateAsync(id, input);

            if (rows == 0)
                return NotFound();

            return Ok(new { message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(new { message = "Xóa thành công" });
        }

        [HttpGet("search-elastic")]
        public async Task<IActionResult> SearchElastic([FromQuery] string keyword)
        {
            var result = await _productService.SearchProductsElasticAsync(keyword);
            return Ok(result);
        }

        [HttpPost("sync-elastic")]
        public async Task<IActionResult> SyncElastic()
        {
            await _productService.SyncProductsToElasticAsync();
            return Ok(new { message = "Đồng bộ product sang Elasticsearch thành công" });
        }
    }
}