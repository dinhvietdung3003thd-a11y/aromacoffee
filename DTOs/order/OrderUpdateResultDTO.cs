namespace WebApplication1.DTOs.order
{
    public class OrderUpdateResultDTO
    {
        public bool Updated { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<OrderCompletionWarningDTO> Warnings { get; set; } = new();
    }
}
