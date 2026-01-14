using Dapper;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Models;
using WebApplication1.services.interfaces;
using System;
using WebApplication1.DTOs.account;

namespace WebApplication1.services
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnection _db;

        public AuthService(IDbConnection db) => _db = db;
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public async Task<AccountDTO?> LoginAsync(LoginRequest request)
        {
            string hashedInput = HashPassword(request.Password);
            Console.WriteLine($"Username: {request.Username}");
            Console.WriteLine($"Password Hash: {HashPassword(request.Password)}");
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
    }
}