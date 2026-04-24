using Dapper;
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
                    new Claim(ClaimTypes.Role, user.Role ?? "Staff"),
                    new Claim("tokenVersion", user.TokenVersion.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateCustomerJwtToken(Customer customer)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:Key"];
            var key = Encoding.UTF8.GetBytes(secretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                    new Claim(ClaimTypes.Name, customer.Username ?? string.Empty),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("tokenVersion", customer.TokenVersion.ToString())
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
                                is_active AS IsActive,
                                token_version AS TokenVersion
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

        public async Task<int> SetupFirstAdminAsync(SetupFirstAdminRequest request)
        {
            const string lockName = "setup_first_admin_lock";
            const int lockTimeoutSeconds = 10;

            if (_db.State != ConnectionState.Open)
                _db.Open();

            var lockResult = await _db.ExecuteScalarAsync<long?>(
                "SELECT GET_LOCK(@name, @timeout);",
                new { name = lockName, timeout = lockTimeoutSeconds });

            if (lockResult != 1)
                throw new InvalidOperationException("Không thể thiết lập admin đầu tiên vào lúc này.");

            using var transaction = _db.BeginTransaction();
            try
            {
                var adminCount = await _db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM users WHERE role = 'Admin'",
                    transaction: transaction);

                if (adminCount > 0)
                {
                    transaction.Commit();
                    return -1; // đã có admin đầu tiên
                }

                var existingUser = await _db.QueryFirstOrDefaultAsync<Account>(
                    "SELECT * FROM users WHERE username = @u",
                    new { u = request.Username },
                    transaction);

                if (existingUser != null)
                {
                    transaction.Commit();
                    return -2; // username đã tồn tại
                }

                string hashedPassword = HashPassword(request.Password);

                string sql = @"INSERT INTO users (username, password_hash, full_name, role, phone_number, is_active) 
                               VALUES (@Username, @PasswordHash, @FullName, 'Admin', @PhoneNumber, 1)";

                var result = await _db.ExecuteAsync(sql, new
                {
                    request.Username,
                    PasswordHash = hashedPassword,
                    request.FullName,
                    request.PhoneNumber
                }, transaction);

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                await _db.ExecuteAsync("SELECT RELEASE_LOCK(@name);", new { name = lockName });
            }
        }

        public async Task<int> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _db.QueryFirstOrDefaultAsync<Account>(
                "SELECT * FROM users WHERE username = @u",
                new { u = request.Username });

            if (existingUser != null)
                return -1;

            string hashedPassword = HashPassword(request.Password);

            string sql = @"INSERT INTO users (username, password_hash, full_name, role, phone_number, is_active) 
                           VALUES (@Username, @PasswordHash, @FullName, @Role, @PhoneNumber, 1)";

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
                "SELECT customer_id FROM customers WHERE username = @u",
                new { u = request.Username });

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
                                created_at AS CreatedAt,
                                token_version AS TokenVersion
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

            string token = GenerateCustomerJwtToken(customer);

            return new CustomerAccountDTO
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                LoyaltyPoints = customer.LoyaltyPoints,
                Token = token
            };
        }

        public async Task<ChangePasswordResponse> ChangePasswordAsync(int actorId, string role, ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentException("Dữ liệu không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.ConfirmPassword))
                throw new ArgumentException("Vui lòng nhập đầy đủ thông tin mật khẩu.");

            if (request.NewPassword.Length < 6)
                throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự.");

            if (request.NewPassword != request.ConfirmPassword)
                throw new ArgumentException("Xác nhận mật khẩu không khớp.");

            if (request.NewPassword == request.CurrentPassword)
                throw new ArgumentException("Mật khẩu mới không được trùng mật khẩu hiện tại.");

            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                var customer = await _db.QueryFirstOrDefaultAsync<Customer>(
                    @"SELECT customer_id AS CustomerId,
                             username AS Username,
                             password_hash AS PasswordHash,
                             full_name AS FullName,
                             loyalty_points AS LoyaltyPoints,
                             token_version AS TokenVersion
                      FROM customers
                      WHERE customer_id = @Id",
                    new { Id = actorId });

                if (customer == null || string.IsNullOrEmpty(customer.PasswordHash) || string.IsNullOrEmpty(customer.Username))
                    throw new UnauthorizedAccessException("Không tìm thấy tài khoản.");

                if (!VerifyPassword(request.CurrentPassword, customer.PasswordHash))
                    throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");

                if (VerifyPassword(request.NewPassword, customer.PasswordHash))
                    throw new ArgumentException("Mật khẩu mới không được trùng mật khẩu hiện tại.");

                string hashedPassword = HashPassword(request.NewPassword);

                var rows = await _db.ExecuteAsync(
                    @"UPDATE customers
                      SET password_hash = @PasswordHash,
                          token_version = token_version + 1
                      WHERE customer_id = @Id",
                    new { PasswordHash = hashedPassword, Id = actorId });

                if (rows == 0)
                    throw new InvalidOperationException("Không thể cập nhật mật khẩu.");

                var updatedCustomer = await _db.QueryFirstOrDefaultAsync<Customer>(
                    @"SELECT customer_id AS CustomerId,
                             username AS Username,
                             token_version AS TokenVersion
                      FROM customers
                      WHERE customer_id = @Id",
                    new { Id = actorId });

                if (updatedCustomer == null || string.IsNullOrEmpty(updatedCustomer.Username))
                    throw new InvalidOperationException("Không thể tạo token mới.");

                string newToken = GenerateCustomerJwtToken(updatedCustomer);

                return new ChangePasswordResponse
                {
                    Message = "Đổi mật khẩu thành công.",
                    Token = newToken
                };
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                var user = await _db.QueryFirstOrDefaultAsync<Account>(
                    @"SELECT user_id AS UserId,
                             username AS Username,
                             password_hash AS PasswordHash,
                             full_name AS FullName,
                             role AS Role,
                             is_active AS IsActive,
                             token_version AS TokenVersion
                      FROM users
                      WHERE user_id = @Id AND is_active = 1",
                    new { Id = actorId });

                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                    throw new UnauthorizedAccessException("Không tìm thấy tài khoản.");

                if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");

                if (VerifyPassword(request.NewPassword, user.PasswordHash))
                    throw new ArgumentException("Mật khẩu mới không được trùng mật khẩu hiện tại.");

                string hashedPassword = HashPassword(request.NewPassword);

                var rows = await _db.ExecuteAsync(
                    @"UPDATE users
                      SET password_hash = @PasswordHash,
                          token_version = token_version + 1
                      WHERE user_id = @Id AND is_active = 1",
                    new { PasswordHash = hashedPassword, Id = actorId });

                if (rows == 0)
                    throw new InvalidOperationException("Không thể cập nhật mật khẩu.");

                var updatedUser = await _db.QueryFirstOrDefaultAsync<Account>(
                    @"SELECT user_id AS UserId,
                             username AS Username,
                             role AS Role,
                             token_version AS TokenVersion
                      FROM users
                      WHERE user_id = @Id AND is_active = 1",
                    new { Id = actorId });

                if (updatedUser == null)
                    throw new InvalidOperationException("Không thể tạo token mới.");

                string newToken = GenerateJwtToken(updatedUser);

                return new ChangePasswordResponse
                {
                    Message = "Đổi mật khẩu thành công.",
                    Token = newToken
                };
            }

            throw new UnauthorizedAccessException("Vai trò không hợp lệ.");
        }

        public async Task<bool> HasAnyAdminAsync()
        {
            var adminCount = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM users WHERE role = 'Admin'");

            return adminCount > 0;
        }
    }
}
