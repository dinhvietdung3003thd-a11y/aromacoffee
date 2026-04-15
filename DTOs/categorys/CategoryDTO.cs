namespace WebApplication1.DTOs.categorys
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }          
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }   
}