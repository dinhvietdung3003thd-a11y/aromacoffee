namespace WebApplication1.Models
{
    public class Account
    {
        public string Username { get; set; } = string.Empty; // Khóa chính (PK)
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
    }
}