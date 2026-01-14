using WebApplication1.DTOs.account;

namespace WebApplication1.services.interfaces
{
    public interface IAuthService
    {
        Task<AccountDTO?> LoginAsync(LoginRequest request);
    }
}