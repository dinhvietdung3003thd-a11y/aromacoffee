using Dapper;
using System.Data;
using WebApplication1.DTOs.inventory;
using WebApplication1.DTOs.report;
using WebApplication1.services.interfaces;
using static Dapper.SqlMapper;

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

        public async Task<bool> CreateTransactionAsync(InventoryTransactionDTO dto)
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

                    var currentStock = await _db.ExecuteScalarAsync<decimal>(
                    "SELECT quantity_in_stock FROM inventory WHERE inventory_id = @id",
                    new { id = dto.InventoryId },
                    transaction);

                    if (dto.TransactionType == "Export" && currentStock < dto.Quantity)
                    {
                        throw new Exception("Không đủ hàng trong kho để xuất.");
                    }

                    // 2. Insert transaction log
                    string insertSql = @"INSERT INTO inventory_transactions 
                             (inventory_id, transaction_type, quantity, user_id, note) 
                             VALUES (@InventoryId, @TransactionType, @Quantity, @UserId, @Note)";

                    await _db.ExecuteAsync(insertSql, dto, transaction);
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

        public async Task<IEnumerable<InventorySummaryReportDTO>> GetMonthlySummaryAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            string sql = @"
        SELECT 
            i.inventory_id AS InventoryId,
            i.name AS InventoryName,

            COALESCE((
                SELECT SUM(
                    CASE 
                        WHEN t.transaction_type = 'Import' THEN t.quantity
                        WHEN t.transaction_type = 'Export' THEN -t.quantity
                        ELSE 0
                    END
                )
                FROM inventory_transactions t
                WHERE t.inventory_id = i.inventory_id
                  AND t.transaction_date < @StartDate
            ), 0) AS OpeningStock,

            COALESCE((
                SELECT SUM(t.quantity)
                FROM inventory_transactions t
                WHERE t.inventory_id = i.inventory_id
                  AND t.transaction_type = 'Import'
                  AND t.transaction_date >= @StartDate
                  AND t.transaction_date < @EndDate
            ), 0) AS TotalImported,

            COALESCE((
                SELECT SUM(t.quantity)
                FROM inventory_transactions t
                WHERE t.inventory_id = i.inventory_id
                  AND t.transaction_type = 'Export'
                  AND t.transaction_date >= @StartDate
                  AND t.transaction_date < @EndDate
            ), 0) AS TotalExported,

            i.quantity_in_stock AS ClosingStock

        FROM inventory i
        ORDER BY i.inventory_id;
    ";

            return await _db.QueryAsync<InventorySummaryReportDTO>(sql, new
            {
                StartDate = startDate,
                EndDate = endDate
            });
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