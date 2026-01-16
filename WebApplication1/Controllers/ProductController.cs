using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.product;
using WebApplication1.Services.interfaces;

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
        public async Task<IActionResult> Create(ProductCreateDTO input) // Dùng DTO mới ở đây
        {
            // Chuyển đổi từ DTO sang Model/DTO gốc để gửi xuống Service
            var productDto = new ProductDTO
            {
                Name = input.Name,
                Price = input.Price,
                CategoryId = input.CategoryId,
                ImageUrl = input.ImageUrl,
                IsAvailable = input.IsAvailable,
            };

            await _productService.AddAsync(productDto);
            return CreatedAtAction(nameof(GetById), new { id = productDto.ProductId }, productDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductUpdateDTO input)
        {
            // 1. Kiểm tra an toàn: ID trên URL phải khớp với ID trong gói tin gửi lên
            if (id != input.ProductId)
            {
                return BadRequest("Mã sản phẩm không khớp!");
            }

            // 2. Chuyển đổi sang ProductDTO để gọi Service
            var productDto = new ProductDTO
            {
                ProductId = input.ProductId,
                Name = input.Name,
                Price = input.Price,
                ImageUrl = input.ImageUrl,
                IsAvailable = input.IsAvailable,
                CategoryId = input.CategoryId
            };

            // 3. Gọi Service cập nhật
            await _productService.UpdateAsync(productDto);

            return Ok(new { message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(new { message = "Xóa thành công" });
        }
    }
}