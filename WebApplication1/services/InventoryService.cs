using Dapper;
using System.Data;
using WebApplication1.DTOs.inventory;
using WebApplication1.DTOs.report;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class InventoryService : IInventoryService
    {
        private readonly IDbConnection _db;
        public InventoryService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<InventoryDisplayDTO>> GetAllAsync()
        {
            // 1. Đổi ingredient_id thành inventory_id
            // 2. JOIN với bảng suppliers để lấy SupplierName
            string sql = @"
            SELECT 
                i.inventory_id, 
                i.name, 
                i.quantity_in_stock, 
                i.unit, 
                i.min_threshold,
                s.name AS SupplierName 
            FROM inventory i
            LEFT JOIN suppliers s ON i.supplier_id = s.supplier_id";

            return await _db.QueryAsync<InventoryDisplayDTO>(sql);
        }

        public async Task<bool> ProcessTransactionAsync(InventoryTransactionDTO dto)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    // 1. Ghi nhật ký vào bảng inventory_transactions kèm theo GIÁ (Price)
                    // Sửa tên cột thành inventory_id cho khớp với Database mới
                    string logSql = @"INSERT INTO inventory_transactions (inventory_id, transaction_type, quantity, price, user_id, note) 
                             VALUES (@InventoryId, @TransactionType, @Quantity, @Price, @UserId, @Note)";
                    await _db.ExecuteAsync(logSql, dto, transaction);

                    // 2. Cập nhật số lượng trong bảng inventory
                    // Sử dụng inventory_id thay vì ingredient_id
                    string op = dto.TransactionType == "Import" ? "+" : "-";
                    string updateSql = $"UPDATE inventory SET quantity_in_stock = quantity_in_stock {op} @Quantity WHERE inventory_id = @InventoryId";

                    await _db.ExecuteAsync(updateSql, new { dto.Quantity, dto.InventoryId }, transaction);

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<InventoryDisplayDTO?> GetByIdAsync(int id)
        {
            // Đồng bộ hóa tên cột inventory_id và thêm JOIN
            string sql = @"
            SELECT i.*, s.name AS SupplierName 
            FROM inventory i
            LEFT JOIN suppliers s ON i.supplier_id = s.supplier_id
            WHERE i.inventory_id = @id";

            return await _db.QueryFirstOrDefaultAsync<InventoryDisplayDTO>(sql, new { id });
        }

        // Các hàm báo cáo của bạn đã khá ổn về mặt logic SQL
        public async Task<IEnumerable<InventorySummaryReportDTO>> GetMonthlySummaryAsync(int month, int year)
        {
            string sql = @"
        SELECT 
            i.inventory_id, 
            i.name as InventoryName, 
            i.unit,
            i.quantity_in_stock as ClosingStock,
            SUM(CASE WHEN t.transaction_type = 'Import' THEN t.quantity ELSE 0 END) as TotalImport,
            SUM(CASE WHEN t.transaction_type = 'Export' THEN t.quantity ELSE 0 END) as TotalExport
        FROM inventory i
        LEFT JOIN inventory_transactions t ON i.inventory_id = t.inventory_id 
            AND MONTH(t.transaction_date) = @Month AND YEAR(t.transaction_date) = @Year
        GROUP BY i.inventory_id, i.name, i.unit, i.quantity_in_stock";

            return await _db.QueryAsync<InventorySummaryReportDTO>(sql, new { Month = month, Year = year });
        }

        public async Task<IEnumerable<SupplierSpendReportDTO>> GetSupplierSpendAsync(int month, int year)
        {
            string sql = @"
        SELECT 
            s.name as SupplierName, 
            SUM(t.quantity * t.price) as TotalSpent,
            COUNT(t.transaction_id) as TransactionCount
        FROM inventory_transactions t
        JOIN inventory i ON t.inventory_id = i.inventory_id
        JOIN suppliers s ON i.supplier_id = s.supplier_id
        WHERE t.transaction_type = 'Import' 
          AND MONTH(t.transaction_date) = @Month 
          AND YEAR(t.transaction_date) = @Year
        GROUP BY s.name";

            return await _db.QueryAsync<SupplierSpendReportDTO>(sql, new { Month = month, Year = year });
        }
    }
}