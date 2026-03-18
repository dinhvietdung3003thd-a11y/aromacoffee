using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.account
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password không được để trống")]
        public string Password { get; set; } = string.Empty;

    }
}