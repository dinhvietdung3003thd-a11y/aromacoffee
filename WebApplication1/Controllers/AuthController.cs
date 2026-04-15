using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.account;
using WebApplication1.services.interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("setup-first-admin")]
        public async Task<IActionResult> SetupFirstAdmin([FromBody] SetupFirstAdminRequest request)
        {
            var result = await _authService.SetupFirstAdminAsync(request);

            if (result == -1)
                return BadRequest(new { message = "Hệ thống đã có admin đầu tiên rồi." });

            if (result == -2)
                return BadRequest(new { message = "Tên tài khoản đã tồn tại." });

            return Ok(new { message = "Tạo admin đầu tiên thành công." });
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result == -1)
                return BadRequest(new { message = "Tên tài khoản đã tồn tại" });

            return Ok(new { message = "Đăng ký nhân viên thành công" });
        }

        [HttpPost("customer/register")]
        public async Task<IActionResult> CustomerRegister([FromBody] CustomerRegisterRequest request)
        {
            var result = await _authService.CustomerRegisterAsync(request);

            if (result == -1)
                return BadRequest(new { message = "Tên tài khoản đã tồn tại!" });

            return Ok(new { message = "Đăng ký thành viên thành công!" });
        }

        [HttpPost("customer/login")]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginRequest request)
        {
            var result = await _authService.CustomerLoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu khách hàng!" });

            return Ok(result);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("has-admin")]
        public async Task<IActionResult> HasAdmin()
        {
            var hasAdmin = await _authService.HasAnyAdminAsync();
            return Ok(new { hasAdmin });
        }
    }
}