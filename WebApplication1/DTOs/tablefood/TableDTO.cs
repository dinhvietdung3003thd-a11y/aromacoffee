public class TableDTO
{
    public int TableId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Available"; // Khớp với ENUM SQL
}