using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
            
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        private bool VerifyPassword(string inputPassword, string hashedPasswordFromDb)
        {
            // Băm mật khẩu người dùng vừa nhập
            string hashedInput = HashPassword(inputPassword);

            // So sánh chuỗi vừa băm với chuỗi trong Database (không phân biệt hoa thường)
            return string.Equals(hashedInput, hashedPasswordFromDb, StringComparison.OrdinalIgnoreCase);
        }
        private string GenerateJwtToken(Account user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "Chuoi_Key_Bao_Mat_Cua_Aroma_Cafe_2026");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
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

        // 1. Đổi kiểu trả về thành Task<object?>
        public async Task<object?> LoginAsync(LoginRequest request)
        {
            string hashedInput = HashPassword(request.Password);
            string sql = "SELECT * FROM users WHERE username = @u AND password_hash = @p AND is_active = 1";

            var user = await _db.QueryFirstOrDefaultAsync<Account>(sql, new { u = request.Username, p = hashedInput });
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return null;
            if (user == null) return null;

            // 2. Tạo Token bằng hàm nội bộ đã viết
            var token = GenerateJwtToken(user);

            // 3. Trả về đối tượng ẩn danh (Anonymous Object) chứa cả Token
            return new
            {
                Token = token, // Đây là phần quan trọng nhất để phân quyền
                User = new
                {
                    user.UserId,
                    user.FullName,
                    user.Role, // Vai trò Admin/Staff để kiểm tra quyền
                    user.PhoneNumber
                }
            };
        }
        public async Task<int> RegisterAsync(RegisterRequest request)
        {
            // 1. Kiểm tra username đã tồn tại chưa
            var existingUser = await _db.QueryFirstOrDefaultAsync<Account>(
                "SELECT * FROM users WHERE username = @u", new { u = request.Username });

            if (existingUser != null) return -1; // Đã tồn tại

            // 2. Băm mật khẩu và lưu
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

        // Thêm logic cho khách hàng vào AuthService
        public async Task<int> CustomerRegisterAsync(CustomerRegisterRequest request)
        {
            // 1. Kiểm tra tài khoản tồn tại trong bảng customers
            var existing = await _db.QueryFirstOrDefaultAsync(
                "SELECT customer_id FROM customers WHERE username = @u", new { u = request.Username });
            if (existing != null) return -1;

            // 2. Băm mật khẩu (Dùng chung hàm SHA256 đã có)
            string hashedPassword = HashPassword(request.Password);

            // 3. Lưu vào database
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
            string hashedInput = HashPassword(request.Password); //

            // Truy vấn thông tin từ bảng khách hàng
            string sql = @"SELECT customer_id, username, full_name, loyalty_points 
                   FROM customers 
                   WHERE username = @u AND password_hash = @p";

            return await _db.QueryFirstOrDefaultAsync<CustomerAccountDTO>(sql, new { u = request.Username, p = hashedInput });
        }
    }
}