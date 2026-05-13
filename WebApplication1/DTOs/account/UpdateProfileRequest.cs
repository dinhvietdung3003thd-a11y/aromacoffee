using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.account
{
    public class UpdateProfileRequest
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }
    }
}