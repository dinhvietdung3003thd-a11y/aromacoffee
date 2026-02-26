namespace WebApplication1.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int userId { get; set; } // ID người tạo
        public int? TableId { get; set; }
        public string? Status { get; set; }
        public int? CustomerId { get; set; }
        public string? Note { get; set; }
        // --- CÁC THUỘC TÍNH NÂNG CẤP CHO GIAO HÀNG ---
        public string? ShippingAddress { get; set; }
        // Tọa độ để hiển thị trên bản đồ
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        // Quãng đường tính bằng km (Dùng decimal để chính xác số lẻ)
        public decimal? DistanceKm { get; set; }
        // Phí vận chuyển
        public decimal ShippingFee { get; set; } = 0;
        // ID của nhân viên giao hàng (Liên kết với bảng Users)
        public int? ShipperId { get; set; }
    }
}