using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.supplier
{
    public class SupplierDTO
    {
        [Required(ErrorMessage = "Name không được để trống")]
        [StringLength(100, ErrorMessage = "Name tối đa 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "ContactName tối đa 100 ký tự")]
        public string? ContactName { get; set; }

        [Phone(ErrorMessage = "Phone không hợp lệ")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Address tối đa 255 ký tự")]
        public string? Address { get; set; }
    }
}