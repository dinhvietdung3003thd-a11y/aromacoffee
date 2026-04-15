using Dapper;
using System.Data;
using WebApplication1.DTOs.tablefood;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class TableService : ITableService
    {
        private readonly IDbConnection _db;
        public TableService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<TableDTO>> GetAllDisplayAsync() =>
            await _db.QueryAsync<TableDTO>("SELECT * FROM tables");

        public async Task<TableDTO?> GetByIdAsync(int id) =>
            await _db.QueryFirstOrDefaultAsync<TableDTO>("SELECT * FROM tables WHERE table_id = @id", new { id });

        public async Task<int> AddAsync(TableCreateDTO dto)
        {
            string sql = "INSERT INTO tables (name, status) VALUES (@Name, @Status)";
            return await _db.ExecuteAsync(sql, dto);
        }

        public async Task<int> UpdateAsync(int id, TableDTO dto)
        {
            string sql = @"UPDATE tables
                   SET name = @Name,
                       status = @Status
                   WHERE table_id = @Id";

            return await _db.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.Status
            });
        }

        public async Task<int> DeleteAsync(int id) =>
            await _db.ExecuteAsync("DELETE FROM tables WHERE table_id = @id", new { id });

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            string sql = "UPDATE tables SET status = @status WHERE table_id = @id";
            return await _db.ExecuteAsync(sql, new { id, status });
        }
    }
}