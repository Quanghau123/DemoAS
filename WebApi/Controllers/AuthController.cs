using Microsoft.AspNetCore.Mvc;

using DemoEF.Application.Interfaces;
using DemoEF.Application.DTOs.Auth;
using DemoEF.Common;

using System.Security.Claims;

namespace DemoEF.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest data)
        {
            var result = await _authService.HandleUserLoginAsync(data);
            return Ok(new ApiResponse<LoginResponseDto>(true, "Login successful.", result));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto data)
        {
            var result = await _authService.RefreshTokenAsync(data.RefreshToken);
            return Ok(new ApiResponse<TokenResponseDto>(true, "Token refreshed successfully.", result));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _authService.LogoutAsync(userId);

            return Ok(new ApiResponse<object>(true, "Logout successful."));
        }
    }
}