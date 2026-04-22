using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.categorys
{
    public class CategoryCreateDTO
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
