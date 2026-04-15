namespace WebApplication1.DTOs.account
{
    public class AccountDTO
    {
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }

        // Bạn có thể thêm Token vào đây nếu sau này dùng JWT
        public string? Token { get; set; }
        public int UserId { get; set; }
    }
}