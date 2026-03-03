using Dapper;
using System.Data;
using WebApplication1.Models;
using WebApplication1.DTOs.inventory;
using WebApplication1.services.interfaces;

namespace WebApplication1.services 
{
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IDbConnection _db;
        public InventoryTransactionService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<InventoryTransaction>> GetAllAsync()
            => await _db.QueryAsync<InventoryTransaction>("SELECT * FROM inventory_transactions ORDER BY transaction_date DESC");

        public async Task<InventoryTransaction?> GetByIdAsync(int id)
            => await _db.QueryFirstOrDefaultAsync<InventoryTransaction>("SELECT * FROM inventory_transactions WHERE transaction_id = @id", new { id });

        public async Task<int> AddAsync(InventoryTransaction entity)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            if (entity.Quantity <= 0)
                throw new ArgumentException("Quantity phải lớn hơn 0.");

            if (entity.TransactionType != "Import" && entity.TransactionType != "Export")
                throw new ArgumentException("TransactionType không hợp lệ.");

            using var transaction = _db.BeginTransaction();
            try
            {
                // 1. Lấy tồn kho hiện tại
                var currentStock = await _db.ExecuteScalarAsync<decimal>(
                    "SELECT quantity_in_stock FR    OM inventory WHERE inventory_id = @id",
                    new { id = entity.InventoryId },
                    transaction);

                if (entity.TransactionType == "Export" && currentStock < entity.Quantity)
                {
                    throw new Exception("Không đủ hàng trong kho để xuất.");
                }

                // 2. Insert transaction log
                string insertSql = @"INSERT INTO inventory_transactions 
                             (inventory_id, transaction_type, quantity, user_id, note) 
                             VALUES (@InventoryId, @TransactionType, @Quantity, @UserId, @Note)";

                await _db.ExecuteAsync(insertSql, entity, transaction);

                // 3. Update tồn kho
                string updateSql;

                if (entity.TransactionType == "Import")
                {
                    updateSql = @"UPDATE inventory 
                          SET quantity_in_stock = quantity_in_stock + @qty 
                          WHERE inventory_id = @id";
                }
                else
                {
                    updateSql = @"UPDATE inventory 
                          SET quantity_in_stock = quantity_in_stock - @qty 
                          WHERE inventory_id = @id AND quantity_in_stock >= @qty";
                }

                await _db.ExecuteAsync(updateSql,
                    new { qty = entity.Quantity, id = entity.InventoryId },
                    transaction);

                transaction.Commit();
                return 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<InventoryTransaction>> SearchAsync(string keyword)
        {
            string sql = "SELECT * FROM inventory_transactions WHERE note LIKE @k OR transaction_type LIKE @k";
            return await _db.QueryAsync<InventoryTransaction>(sql, new { k = $"%{keyword}%" });
        }

        public async Task<IEnumerable<InventoryTransactionDisplayDTO>> GetAllDisplayAsync()
        {
            // JOIN để lấy thông tin đầy đủ cho quản lý
            string sql = @"SELECT t.transaction_id, i.name as InventoryName, t.transaction_type, 
                              t.quantity, i.unit, t.transaction_date, u.full_name as StaffName, t.note
                       FROM inventory_transactions t
                       JOIN inventory i ON t.inventory_id = i.inventory_id
                       JOIN users u ON t.user_id = u.user_id
                       ORDER BY t.transaction_date DESC";
            return await _db.QueryAsync<InventoryTransactionDisplayDTO>(sql);
        }
    }
}

