using Dapper;
using System.Data;
using WebApplication1.DTOs;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class TableService : ITableService
    {
        private readonly IDbConnection _db;
        public TableService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<TableDTO>> GetAllAsync() =>
            await _db.QueryAsync<TableDTO>("SELECT * FROM tables");

        public async Task<TableDTO?> GetByIdAsync(int id) =>
            await _db.QueryFirstOrDefaultAsync<TableDTO>("SELECT * FROM tables WHERE table_id = @id", new { id });

        public async Task<int> AddAsync(TableDTO entity)
        {
            string sql = "INSERT INTO tables (name, status) VALUES (@Name, @Status)";
            return await _db.ExecuteAsync(sql, entity);
        }

        public async Task<int> UpdateAsync(TableDTO entity)
        {
            string sql = "UPDATE tables SET name = @Name, status = @Status WHERE table_id = @TableId";
            return await _db.ExecuteAsync(sql, entity);
        }

        public async Task<int> DeleteAsync(int id) =>
            await _db.ExecuteAsync("DELETE FROM tables WHERE table_id = @id", new { id });

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            string sql = "UPDATE tables SET status = @status WHERE table_id = @id";
            return await _db.ExecuteAsync(sql, new { id, status });
        }

        public async Task<IEnumerable<TableDTO>> SearchAsync(string keyword) =>
            await _db.QueryAsync<TableDTO>("SELECT * FROM tables WHERE name LIKE @key OR status LIKE @key", new { key = $"%{keyword}%" });
    }
}