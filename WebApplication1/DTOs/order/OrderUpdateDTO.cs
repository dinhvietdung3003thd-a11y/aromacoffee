namespace WebApplication1.DTOs.order
{
    // Kế thừa từ OrderCreateDTO để lấy lại các trường OrderDate, Details, Note...
    public class OrderUpdateDTO : OrderCreateDTO
    {
        public int OrderId { get; set; } // Chỉ thêm trường này cho việc Update
    }
}