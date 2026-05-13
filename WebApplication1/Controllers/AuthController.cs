using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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


        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(idClaim, out var actorId) || string.IsNullOrWhiteSpace(roleClaim))
                return Unauthorized(new { message = "Token không hợp lệ." });

            try
            {
                var result = await _authService.ChangePasswordAsync(actorId, roleClaim, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }

            return int.Parse(userIdClaim.Value);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();

            var profile = await _authService.GetProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(profile);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(
    [FromBody] UpdateProfileRequest request
)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();

            var updatedProfile =
                await _authService.UpdateProfileAsync(userId, request);

            if (updatedProfile == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(updatedProfile);
        }
    }
}