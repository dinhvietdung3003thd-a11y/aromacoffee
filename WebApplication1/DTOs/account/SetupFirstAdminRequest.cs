using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.account
{
    public class SetupFirstAdminRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }
    }
}