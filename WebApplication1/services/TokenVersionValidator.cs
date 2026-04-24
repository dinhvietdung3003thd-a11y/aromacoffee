using Dapper;
using System.Data;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    public class TokenVersionValidator : ITokenVersionValidator
    {
        private readonly IDbConnection _db;

        public TokenVersionValidator(IDbConnection db)
        {
            _db = db;
        }

        public async Task<bool> ValidateAsync(int actorId, string role, int tokenVersion)
        {
            if (actorId <= 0) return false;

            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                var dbVersion = await _db.ExecuteScalarAsync<int?>(
                    "SELECT token_version FROM customers WHERE customer_id = @Id",
                    new { Id = actorId });

                return dbVersion.HasValue && dbVersion.Value == tokenVersion;
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                var row = await _db.QueryFirstOrDefaultAsync<(int TokenVersion, bool IsActive)>(
                    @"SELECT token_version AS TokenVersion, is_active AS IsActive
                      FROM users
                      WHERE user_id = @Id",
                    new { Id = actorId });

                if (row == default) return false;
                if (!row.IsActive) return false;

                return row.TokenVersion == tokenVersion;
            }

            return false;
        }
    }
}
