using Dapper;
using System.Data;
using WebApplication1.DTOs.inventorys;

using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class InventoryService : IInventoryService
    {
        private readonly IDbConnection _db;

        public InventoryService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<InventoryDisplayDTO>> GetAllAsync()
        {
            const string sql = @"
                SELECT
                    i.inventory_id AS InventoryId,
                    i.name AS Name,
                    i.quantity_in_stock AS QuantityInStock,
                    i.unit AS Unit,
                    i.min_threshold AS MinThreshold,
                    s.name AS SupplierName
                FROM inventory i
                LEFT JOIN suppliers s ON i.supplier_id = s.supplier_id
                ORDER BY i.inventory_id ASC;";

            return await _db.QueryAsync<InventoryDisplayDTO>(sql);
        }

        public async Task<InventoryDisplayDTO?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    i.inventory_id AS InventoryId,
                    i.name AS Name,
                    i.quantity_in_stock AS QuantityInStock,
                    i.unit AS Unit,
                    i.min_threshold AS MinThreshold,
                    s.name AS SupplierName
                FROM inventory i
                LEFT JOIN suppliers s ON i.supplier_id = s.supplier_id
                WHERE i.inventory_id = @Id;";

            return await _db.QueryFirstOrDefaultAsync<InventoryDisplayDTO>(sql, new { Id = id });
        }

        public async Task<bool> CreateTransactionAsync(InventoryTransactionDTO dto)
        {
            if (dto == null)
                throw new ArgumentException("Dữ liệu giao dịch kho không hợp lệ.");

            if (dto.InventoryId <= 0 || dto.Quantity <= 0 || string.IsNullOrWhiteSpace(dto.TransactionType))
                throw new ArgumentException("Thông tin giao dịch kho không hợp lệ.");

            string transactionType = dto.TransactionType.Trim();

            if (transactionType != "Import" && transactionType != "Export")
                throw new ArgumentException("Loại giao dịch phải là Import hoặc Export.");

            if (_db.State != ConnectionState.Open)
                _db.Open();

            using var transaction = _db.BeginTransaction();

            try
            {
                const string getInventorySql = @"
                    SELECT quantity_in_stock
                    FROM inventory
                    WHERE inventory_id = @InventoryId;";

                var currentStock = await _db.QueryFirstOrDefaultAsync<decimal?>(
                    getInventorySql,
                    new { dto.InventoryId },
                    transaction
                );

                if (currentStock == null)
                    throw new KeyNotFoundException("Nguyên liệu không tồn tại.");

                if (transactionType == "Export" && currentStock.Value < dto.Quantity)
                    throw new InvalidOperationException("Số lượng tồn kho không đủ để xuất.");

                string updateStockSql = transactionType == "Import"
                    ? @"
                        UPDATE inventory
                        SET quantity_in_stock = quantity_in_stock + @Quantity
                        WHERE inventory_id = @InventoryId;"
                    : @"
                        UPDATE inventory
                        SET quantity_in_stock = quantity_in_stock - @Quantity
                        WHERE inventory_id = @InventoryId;";

                int updatedRows = await _db.ExecuteAsync(
                    updateStockSql,
                    new
                    {
                        dto.InventoryId,
                        dto.Quantity
                    },
                    transaction
                );

                if (updatedRows == 0)
                    throw new InvalidOperationException("Không thể cập nhật tồn kho.");

                const string insertTransactionSql = @"
                    INSERT INTO inventory_transactions
                    (
                        inventory_id,
                        transaction_type,
                        quantity,
                        price,
                        user_id,
                        note
                    )
                    VALUES
                    (
                        @InventoryId,
                        @TransactionType,
                        @Quantity,
                        @Price,
                        @UserId,
                        @Note
                    );";

                await _db.ExecuteAsync(
                    insertTransactionSql,
                    new
                    {
                        dto.InventoryId,
                        TransactionType = transactionType,
                        dto.Quantity,
                        dto.Price,
                        dto.UserId,
                        dto.Note
                    },
                    transaction
                );

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<InventorySummaryReportDTO>> GetMonthlySummaryAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            const string sql = @"
                SELECT
                    i.inventory_id AS InventoryId,
                    i.name AS InventoryName,
                    i.unit AS Unit,

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
                    ), 0) AS TotalImport,

                    COALESCE((
                        SELECT SUM(t.quantity)
                        FROM inventory_transactions t
                        WHERE t.inventory_id = i.inventory_id
                          AND t.transaction_type = 'Export'
                          AND t.transaction_date >= @StartDate
                          AND t.transaction_date < @EndDate
                    ), 0) AS TotalExport,

                    i.quantity_in_stock AS ClosingStock
                FROM inventory i
                ORDER BY i.inventory_id ASC;";

            return await _db.QueryAsync<InventorySummaryReportDTO>(
                sql,
                new
                {
                    StartDate = startDate,
                    EndDate = endDate
                }
            );
        }

        public async Task<IEnumerable<SupplierSpendReportDTO>> GetSupplierSpendAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            const string sql = @"
                SELECT
                    s.name AS SupplierName,
                    COALESCE(SUM(t.quantity * t.price), 0) AS TotalSpent,
                    COUNT(t.transaction_id) AS TransactionCount
                FROM inventory_transactions t
                INNER JOIN inventory i ON t.inventory_id = i.inventory_id
                INNER JOIN suppliers s ON i.supplier_id = s.supplier_id
                WHERE t.transaction_type = 'Import'
                  AND t.transaction_date >= @StartDate
                  AND t.transaction_date < @EndDate
                GROUP BY s.name
                ORDER BY s.name ASC;";

            return await _db.QueryAsync<SupplierSpendReportDTO>(
                sql,
                new
                {
                    StartDate = startDate,
                    EndDate = endDate
                }
            );
        }
        public async Task<int> CreateAsync(InventoryCreateDTO dto)
        {
            if (dto == null)
                throw new Exception("Dữ liệu không hợp lệ.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Tên nguyên liệu không được để trống.");

            if (string.IsNullOrWhiteSpace(dto.Unit))
                throw new Exception("Đơn vị không được để trống.");

            if (dto.QuantityInStock < 0)
                throw new Exception("Số lượng tồn không được âm.");

            if (dto.MinThreshold < 0)
                throw new Exception("Ngưỡng tối thiểu không được âm.");

            const string sql = @"
        INSERT INTO inventory
        (
            name,
            unit,
            quantity_in_stock,
            min_threshold,
            supplier_id,
            updated_at
        )
        VALUES
        (
            @Name,
            @Unit,
            @QuantityInStock,
            @MinThreshold,
            @SupplierId,
            NOW()
        );
        SELECT LAST_INSERT_ID();";

            var id = await _db.ExecuteScalarAsync<int>(sql, new
            {
                dto.Name,
                dto.Unit,
                dto.QuantityInStock,
                dto.MinThreshold,
                dto.SupplierId
            });

            return id;
        }
    }
}
