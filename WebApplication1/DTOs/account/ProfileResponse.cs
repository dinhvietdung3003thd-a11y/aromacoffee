namespace WebApplication1.DTOs.account
{
    public class ProfileResponse
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? AvatarUrl { get; set; }
    }
}