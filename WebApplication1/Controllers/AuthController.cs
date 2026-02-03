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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result == -1) return BadRequest("Tên tài khoản đã tồn tại");
        return Ok(new { message = "Đăng ký nhân viên thành công" });
    }

    // Đăng ký cho Khách hàng
    [HttpPost("customer/register")]
    public async Task<IActionResult> CustomerRegister([FromBody] CustomerRegisterRequest request)
    {
        var result = await _authService.CustomerRegisterAsync(request);
        if (result == -1) return BadRequest(new { message = "Tên tài khoản đã tồn tại!" });
        return Ok(new { message = "Đăng ký thành viên thành công!" });
    }

    // Đăng nhập cho Khách hàng
    [HttpPost("customer/login")]
    public async Task<IActionResult> CustomerLogin([FromBody] LoginRequest request)
    {
        var result = await _authService.CustomerLoginAsync(request);
        if (result == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu khách hàng!" });

        return Ok(result);
    }
    // vi du cho viec tao file view 
    // tao ham tai file controller r chuot phai ten ham tao file view 
    //ae co the run tren web local de test xem no can nhap nhung cai j cho muc day ma thiet ke nhe 
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index() { return View(); }
}