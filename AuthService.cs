using Dapper;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnection _db;

        public AuthService(IDbConnection db) => _db = db;

        public async Task<AccountDTO?> LoginAsync(LoginRequest request)
        {
            string hashedInput = HashPassword(request.Password);
            string sql = "SELECT * FROM accounts WHERE Username = @u AND Password = @p";

            var user = await _db.QueryFirstOrDefaultAsync<Account>(sql, new { u = request.Username, p = hashedInput });

            if (user == null) return null;

            return new AccountDTO
            {
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}