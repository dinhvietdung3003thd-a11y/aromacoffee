using Microsoft.AspNetCore.Mvc;
using WebApplication1.services.interfaces;
using WebApplication1.view;
using WebApplication1.DTOs.account;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

        return Ok(result);
    }
    // vi du cho viec tao file view 
    // tao ham tai file controller r chuot phai ten ham tao file view 
    //ae co the run tren web local de test xem no can nhap nhung cai j cho muc day ma thiet ke nhe 
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index() { return View(); }
}