using Dapper;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.DTOs.account;
using WebApplication1.Models;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnection _db;
        private readonly IConfiguration _configuration;

        public AuthService(IDbConnection db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string GenerateJwtToken(Account user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:Key"];
            var key = Encoding.UTF8.GetBytes(secretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? "Staff")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateCustomerJwtToken(int customerId, string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:Key"];
            var key = Encoding.UTF8.GetBytes(secretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Customer")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<object?> LoginAsync(LoginRequest request)
        {
            string sql = @"SELECT 
                                user_id AS UserId,
                                username AS Username,
                                password_hash AS PasswordHash,
                                full_name AS FullName,
                                role AS Role,
                                phone_number AS PhoneNumber,
                                is_active AS IsActive
                           FROM users
                           WHERE username = @u AND is_active = 1";

            var user = await _db.QueryFirstOrDefaultAsync<Account>(sql, new
            {
                u = request.Username
            });

            if (user == null) return null;
            if (string.IsNullOrEmpty(user.PasswordHash)) return null;

            bool isPasswordValid = VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid) return null;

            var token = GenerateJwtToken(user);

            return new
            {
                Token = token,
                User = new
                {
                    user.UserId,
                    user.FullName,
                    user.Role,
                    user.PhoneNumber
                }
            };
        }

        public async Task<int> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _db.QueryFirstOrDefaultAsync<Account>(
                "SELECT * FROM users WHERE username = @u", new { u = request.Username });

            if (existingUser != null) return -1;

            string hashedPassword = HashPassword(request.Password);

            string sql = @"INSERT INTO users (username, password_hash, full_name, role, phone_number) 
                           VALUES (@Username, @PasswordHash, @FullName, @Role, @PhoneNumber)";

            return await _db.ExecuteAsync(sql, new
            {
                request.Username,
                PasswordHash = hashedPassword,
                request.FullName,
                request.Role,
                request.PhoneNumber
            });
        }

        public async Task<int> CustomerRegisterAsync(CustomerRegisterRequest request)
        {
            var existing = await _db.QueryFirstOrDefaultAsync(
                "SELECT customer_id FROM customers WHERE username = @u", new { u = request.Username });

            if (existing != null) return -1;

            string hashedPassword = HashPassword(request.Password);

            string sql = @"INSERT INTO customers (username, password_hash, full_name, phone_number, email, loyalty_points) 
                           VALUES (@Username, @PasswordHash, @FullName, @PhoneNumber, @Email, 0)";

            return await _db.ExecuteAsync(sql, new
            {
                request.Username,
                PasswordHash = hashedPassword,
                request.FullName,
                request.PhoneNumber,
                request.Email
            });
        }

        public async Task<CustomerAccountDTO?> CustomerLoginAsync(LoginRequest request)
        {
            string sql = @"SELECT 
                                customer_id AS CustomerId,
                                username AS Username,
                                password_hash AS PasswordHash,
                                full_name AS FullName,
                                loyalty_points AS LoyaltyPoints,
                                phone_number AS PhoneNumber,
                                email AS Email,
                                created_at AS CreatedAt
                           FROM customers
                           WHERE username = @u";

            var customer = await _db.QueryFirstOrDefaultAsync<Customer>(sql, new
            {
                u = request.Username
            });

            if (customer == null) return null;
            if (string.IsNullOrEmpty(customer.PasswordHash) || string.IsNullOrEmpty(customer.Username))
                return null;

            bool isPasswordValid = VerifyPassword(request.Password, customer.PasswordHash);
            if (!isPasswordValid) return null;

            string token = GenerateCustomerJwtToken(customer.CustomerId, customer.Username);

            return new CustomerAccountDTO
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                LoyaltyPoints = customer.LoyaltyPoints,
                Token = token
            };
        }
    }
}