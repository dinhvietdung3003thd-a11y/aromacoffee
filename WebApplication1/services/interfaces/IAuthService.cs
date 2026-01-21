using WebApplication1.DTOs.account;

namespace WebApplication1.services.interfaces
{
    public interface IAuthService
    {
        Task<object?> LoginAsync(LoginRequest request);
        Task<int> RegisterAsync( RegisterRequest request);

        Task<CustomerAccountDTO?> CustomerLoginAsync(LoginRequest request);
        Task<int> CustomerRegisterAsync(CustomerRegisterRequest request);
    }
}