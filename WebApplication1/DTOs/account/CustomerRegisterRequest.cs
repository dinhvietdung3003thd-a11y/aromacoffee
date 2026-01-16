namespace WebApplication1.DTOs.account
{
    public class CustomerRegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } 
        public string? Email { get; set; } 
    }
}