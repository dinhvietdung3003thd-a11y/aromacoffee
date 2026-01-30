using Dapper;
using System.Data;
using WebApplication1.DTOs.supplier;
using WebApplication1.Models;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class SupplierService : ISupplierService
    {
        private readonly IDbConnection _db;
        public SupplierService(IDbConnection db) => _db = db;

        public async Task<IEnumerable<Supplier>> GetAllAsync()
            => await _db.QueryAsync<Supplier>("SELECT * FROM suppliers");

        public async Task<Supplier?> GetByIdAsync(int id)
            => await _db.QueryFirstOrDefaultAsync<Supplier>("SELECT * FROM suppliers WHERE supplier_id = @id", new { id });

        public async Task<int> AddAsync(Supplier entity)
        {
            string sql = @"INSERT INTO suppliers (name, contact_name, phone, email, address) 
                       VALUES (@Name, @ContactName, @Phone, @Email, @Address)";
            return await _db.ExecuteAsync(sql, entity);
        }

        public async Task<int> UpdateAsync(Supplier entity)
        {
            string sql = @"UPDATE suppliers 
                       SET name = @Name, contact_name = @ContactName, 
                           phone = @Phone, email = @Email, address = @Address 
                       WHERE supplier_id = @SupplierId";
            return await _db.ExecuteAsync(sql, entity);
        }

        public async Task<int> DeleteAsync(int id)
            => await _db.ExecuteAsync("DELETE FROM suppliers WHERE supplier_id = @id", new { id });

        public async Task<IEnumerable<Supplier>> SearchAsync(string keyword)
        {
            string sql = "SELECT * FROM suppliers WHERE name LIKE @k OR phone LIKE @k OR contact_name LIKE @k";
            return await _db.QueryAsync<Supplier>(sql, new { k = $"%{keyword}%" });
        }
        public async Task<IEnumerable<SupplierDisplayDTO>> GetAllDisplayAsync()
        {
            // SQL lấy dữ liệu và đổi tên cột contact_name thành ContactPerson cho khớp DTO
            string sql = @"SELECT supplier_id, name, phone, contact_name as ContactPerson 
                       FROM suppliers";
            return await _db.QueryAsync<SupplierDisplayDTO>(sql);
        }
    }
}