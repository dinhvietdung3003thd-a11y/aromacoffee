using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.tablefood
{
    public class TableCreateDTO
    {
        [Required(ErrorMessage = "Name không được để trống")]
        [StringLength(50, ErrorMessage = "Name tối đa 50 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status không được để trống")]
        [RegularExpression("Available|Occupied|Reserved", ErrorMessage = "Status không hợp lệ")]
        public string Status { get; set; } = "Available";
    }
}