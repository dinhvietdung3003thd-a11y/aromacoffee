using Dapper;
using System.Data;
using WebApplication1.DTOs;
using WebApplication1.services.interfaces;

namespace WebApplication1.Services
{
    public class TableService : ITableService
    {
        private readonly IDbConnection _db;
        public TableService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<TableDTO>> GetAllAsync()
        {
            return await _db.QueryAsync<TableDTO>("SELECT * FROM tablefood");
        }

        public async Task<TableDTO?> GetByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<TableDTO>("SELECT * FROM tablefood WHERE Id = @id", new { id });
        }

        public async Task<int> AddAsync(TableDTO entity)
        {
            string sql = "INSERT INTO tablefood (Name, Status) VALUES (@Name, @Status)";
            return await _db.ExecuteAsync(sql, entity);
        }

        public async Task<int> UpdateAsync(TableDTO entity)
        {
            string sql = "UPDATE tablefood SET Name = @Name, Status = @Status WHERE Id = @Id";
            return await _db.ExecuteAsync(sql, entity);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _db.ExecuteAsync("DELETE FROM tablefood WHERE Id = @id", new { id });
        }

        public async Task<IEnumerable<TableDTO>> SearchAsync(string keyword)
        {
            string sql = "SELECT * FROM tablefood WHERE Name LIKE @key OR Status LIKE @key";
            return await _db.QueryAsync<TableDTO>(sql, new { key = $"%{keyword}%" });
        }

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            string sql = "UPDATE tablefood SET Status = @status WHERE Id = @id";
            return await _db.ExecuteAsync(sql, new { id, status });
        }
    }
}