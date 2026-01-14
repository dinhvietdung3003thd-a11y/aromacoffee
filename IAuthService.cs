using WebApplication1.DTOs;

namespace WebApplication1.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AccountDTO?> LoginAsync(LoginRequest request);
    }
}