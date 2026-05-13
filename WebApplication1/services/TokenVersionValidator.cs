// TokenVersionValidator.cs
// Validates JWT token version to ensure tokens haven't been invalidated by logout-all or role changes
// Checks user active status and token version match; implements security mechanism for forced token expiration

using Dapper;
using System.Data;
using WebApplication1.services.interfaces;

namespace WebApplication1.services
{
    // Validates JWT token version to ensure token hasn't been invalidated (logout-all mechanism)
    // Checks if token version matches database version and user is still active
    public class TokenVersionValidator : ITokenVersionValidator
    {
        private readonly IDbConnection _db;

        public TokenVersionValidator(IDbConnection db)
        {
            _db = db;
        }

        // Compares JWT token version with database version; mismatch means token was invalidated
        public async Task<bool> ValidateAsync(int actorId, string role, int tokenVersion)
        {
            if (actorId <= 0) return false;

            // For customer accounts, validate against customer token_version
            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                var dbVersion = await _db.ExecuteScalarAsync<int?>(
                    "SELECT token_version FROM customers WHERE customer_id = @Id",
                    new { Id = actorId });

                // Token valid if customer exists and version matches
                return dbVersion.HasValue && dbVersion.Value == tokenVersion;
            }

            // For staff/admin accounts, validate version and active status
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                var row = await _db.QueryFirstOrDefaultAsync<(int TokenVersion, bool IsActive)>(
                    @"SELECT token_version AS TokenVersion, is_active AS IsActive
                      FROM users
                      WHERE user_id = @Id",
                    new { Id = actorId });

                if (row == default) return false;
                // Token invalid if user account is deactivated
                if (!row.IsActive) return false;

                // Token valid only if version matches
                return row.TokenVersion == tokenVersion;
            }

            return false;
        }
    }
}
