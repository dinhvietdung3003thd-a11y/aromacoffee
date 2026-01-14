namespace WebApplication1.Models
{
    public class TableFood
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Trống"; // Mặc định là Trống 
    }
}