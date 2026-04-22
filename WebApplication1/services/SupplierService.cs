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

        public async Task<SupplierDisplayDTO?> GetByIdAsync(int id)
        {
            string sql = @"SELECT supplier_id,
                          name,
                          phone,
                          contact_name AS ContactPerson
                   FROM suppliers
                   WHERE supplier_id = @id";

            return await _db.QueryFirstOrDefaultAsync<SupplierDisplayDTO>(sql, new { id });
        }

        public async Task<int> AddAsync(SupplierDTO dto)
        {
            string sql = @"INSERT INTO suppliers (name, contact_name, phone, email, address) 
                       VALUES (@Name, @ContactName, @Phone, @Email, @Address)";
            return await _db.ExecuteAsync(sql, dto);
        }

        public async Task<int> UpdateAsync(int id, SupplierDTO dto)
        {
            string sql = @"UPDATE suppliers 
                       SET name = @Name, contact_name = @ContactName, 
                           phone = @Phone, email = @Email, address = @Address 
                       WHERE supplier_id = @Id";
            return await _db.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.ContactName,
                dto.Phone,
                dto.Email,
                dto.Address
            });
        }

        public async Task<int> DeleteAsync(int id)
            => await _db.ExecuteAsync("DELETE FROM suppliers WHERE supplier_id = @id", new { id });

        public async Task<IEnumerable<SupplierDisplayDTO>> SearchAsync(string keyword)
        {
            string sql = @"SELECT supplier_id,
                          name,
                          phone,
                          contact_name AS ContactPerson
                   FROM suppliers
                   WHERE name LIKE @k
                      OR phone LIKE @k
                      OR contact_name LIKE @k";

            return await _db.QueryAsync<SupplierDisplayDTO>(sql, new { k = $"%{keyword}%" });
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