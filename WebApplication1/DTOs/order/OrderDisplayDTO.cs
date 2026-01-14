namespace WebApplication1.DTOs.order
{
    public class OrderDisplayDTO
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? TableNumber { get; set; }
        public string? Status { get; set; }

        // Lấy tên đầy đủ của nhân viên từ bảng accounts qua CreatedBy
        public string? CreatorFullName { get; set; }
    }
}