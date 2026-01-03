using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities.Requests.User;
using DemoEF.Application.DTOs.User;
using DemoEF.Common;

using System.Security.Claims;

namespace DemoEF.WebApi.Controllers
{
    /// <summary>
    /// API quản lý người dùng
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="data">Email và Password</param>
        /// <returns>AccessToken, RefreshToken và thông tin user</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/User/login
        ///     {
        ///        "email": "quanghau@gmail.com",
        ///        "password": "Abc@123"
        ///     }
        /// 
        /// admin:
        /// Sau khi đăng nhập thành công:
        /// 1. Copy AccessToken
        /// 2. Click nút "Authorize" ở góc trên bên phải
        /// 3. Nhập "Bearer {AccessToken}" (có dấu cách sau Bearer)
        /// 4. Click "Authorize" để sử dụng các API cần xác thực
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest data)
        {
            var result = await _authService.HandleUserLoginAsync(data.Email, data.Password);
            return Ok(new ApiResponse<LoginResponseDto>(true, "Login successful.", result));
        }

        /// <summary>
        /// Làm mới AccessToken
        /// </summary>
        /// <param name="data">AccessToken cũ và RefreshToken</param>
        /// <returns>AccessToken và RefreshToken mới</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/User/refresh-token
        ///     {
        ///        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///        "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
        ///     }
        ///     
        /// Sử dụng khi AccessToken hết hạn
        /// </remarks>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto data)
        {
            var result = await _authService.RefreshTokenAsync(data.AccessToken, data.RefreshToken);
            return Ok(new ApiResponse<TokenResponseDto>(true, "Token refreshed successfully.", result));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _authService.LogoutAsync(userId);

            return Ok(new ApiResponse<object>(true, "Logout successful."));
        }
    }
}