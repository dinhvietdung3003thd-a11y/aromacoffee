namespace WebApplication1.Common
{
    public static class StatusConstants
    {
        public static readonly HashSet<string> OrderStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending",
            "Paid",
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
