namespace WebApplication1.Models
{
    public class TableFood
    {
        public int TableId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Available"; // Mặc định là Trống 
    }
}