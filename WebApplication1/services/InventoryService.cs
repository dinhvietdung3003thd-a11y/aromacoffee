using Dapper;
using System.Data;
using WebApplication1.DTOs.inventory;

public class InventoryService : IInventoryService
{
    private readonly IDbConnection _db;
    public InventoryService(IDbConnection db) => _db = db;

    public async Task<IEnumerable<InventoryDisplayDTO>> GetAllAsync()
    {
        string sql = "SELECT ingredient_id, name, quantity_in_stock, unit, min_threshold FROM inventory"; //
        return await _db.QueryAsync<InventoryDisplayDTO>(sql);
    }

    public async Task<bool> ProcessTransactionAsync(InventoryTransactionDTO dto)
    {
        if (_db.State != ConnectionState.Open) _db.Open();
        using (var transaction = _db.BeginTransaction())
        {
            try
            {
                // 1. Ghi nhật ký vào bảng inventory_transactions
                string logSql = @"INSERT INTO inventory_transactions (inventory_id, transaction_type, quantity, user_id, note) 
                                 VALUES (@InventoryId, @TransactionType, @Quantity, @UserId, @Note)";
                await _db.ExecuteAsync(logSql, dto, transaction);

                // 2. Cập nhật số lượng trong bảng inventory
                // Nếu là Import thì cộng (+), nếu là Export thì trừ (-)
                string op = dto.TransactionType == "Import" ? "+" : "-";
                string updateSql = $"UPDATE inventory SET quantity_in_stock = quantity_in_stock {op} @Quantity WHERE ingredient_id = @InventoryId";

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
        string sql = "SELECT * FROM inventory WHERE ingredient_id = @id"; //
        return await _db.QueryFirstOrDefaultAsync<InventoryDisplayDTO>(sql, new { id });
    }
}