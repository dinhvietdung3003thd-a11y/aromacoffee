using Dapper;
using System.Data;
using WebApplication1.Models;
using WebApplication1.DTOs.inventory;
using WebApplication1.Services.interfaces;

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
        // Chỉ cho phép INSERT để ghi nhật ký mới
        string sql = @"INSERT INTO inventory_transactions (inventory_id, transaction_type, quantity, user_id, note) 
                       VALUES (@InventoryId, @TransactionType, @Quantity, @UserId, @Note)";
        return await _db.ExecuteAsync(sql, entity);
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