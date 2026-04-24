namespace WebApplication1.services.interfaces
{
    public interface ITokenVersionValidator
    {
        Task<bool> ValidateAsync(int actorId, string role, int tokenVersion);
    }
}
