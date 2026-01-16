namespace WebApplication1.DTOs.account
{
    public class CustomerAccountDTO
    {
        public int CustomerId { get; set; } 
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; 
        public int LoyaltyPoints { get; set; } 
        public string Role { get; } = "Customer"; // Để phân biệt với nhân viên
    }
}