using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.inventorys
{
    public class InventoryCreateDTO
    {
        [Required(ErrorMessage = "Name không được để trống")]
        [StringLength(100, ErrorMessage = "Name tối đa 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit không được để trống")]
        [StringLength(50, ErrorMessage = "Unit tối đa 50 ký tự")]
        public string Unit { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "QuantityInStock phải >= 0")]
        public decimal QuantityInStock { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "MinThreshold phải >= 0")]
        public decimal MinThreshold { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SupplierId không hợp lệ")]
        public int? SupplierId { get; set; }
    }
}