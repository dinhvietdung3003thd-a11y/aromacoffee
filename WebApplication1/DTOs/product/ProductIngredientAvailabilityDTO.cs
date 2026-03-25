namespace WebApplication1.DTOs.product
{
    public class ProductIngredientAvailabilityDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int EstimatedServingsLeft { get; set; }
        public string Status { get; set; } = "available";
        public string WarningMessage { get; set; } = string.Empty;
    }
}
