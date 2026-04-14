namespace WebApplication1.Common
{
    public static class StatusConstants
    {
        public static readonly HashSet<string> OrderStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Chờ xử lý",
            "Đang làm",
            "Đã hoàn thành",
            "Đã hủy",
            "Pending",
            "Completed",
            "Cancelled"
        };

        public static readonly HashSet<string> TableStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Available",
            "Occupied",
            "Reserved"
        };
    }
}
