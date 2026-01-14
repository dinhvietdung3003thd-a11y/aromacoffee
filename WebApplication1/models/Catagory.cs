namespace WebApplication1.Models
{
    public class Catagory
    {
        public int Id { get; set; } // Auto increment
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}