namespace WebApplication1.Models
{
    public class Customer
    {
        public int CustomerId { get; set; } 
        public string FullName { get; set; } = string.Empty; 
        public string? Username { get; set; } 
        public string? PasswordHash { get; set; } 
        public string? PhoneNumber { get; set; } 
        public string? Email { get; set; } 
        public int LoyaltyPoints { get; set; } = 0; 
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
    }
}