namespace WebApplication1.Models
{
    public class Category
    {
        public int CategoryId { get; set; } // Auto increment
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}