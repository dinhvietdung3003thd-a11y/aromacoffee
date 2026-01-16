namespace WebApplication1.Models
{
    public class Account
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty; // Khóa chính (PK)
        public string PasswordHash { get; set; } = string.Empty;
        public string?  FullName { get; set; }
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
    }
}