using WebApplication1.DTOs.account;

namespace WebApplication1.services.interfaces
{
    public interface IAuthService
    {
        Task<Object?> LoginAsync(LoginRequest request);
        Task<int> RegisterAsync( RegisterRequest request);
        Task<CustomerAccountDTO?> CustomerLoginAsync(LoginRequest request);
        Task<int> CustomerRegisterAsync(CustomerRegisterRequest request);
        Task<int> SetupFirstAdminAsync(SetupFirstAdminRequest request);
        Task<ChangePasswordResponse> ChangePasswordAsync(int actorId, string role, ChangePasswordRequest request);
        Task<bool> HasAnyAdminAsync();
        Task<ProfileResponse?> GetProfileAsync(int userId);
        Task<ProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    }
}
