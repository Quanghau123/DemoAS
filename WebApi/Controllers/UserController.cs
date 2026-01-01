using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities.Requests.User;

namespace DemoEF.WebApi.Controllers
{
    /// <summary>
    /// API quản lý người dùng
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <param name="data">Thông tin đăng ký (UserName, Email, Password, UserRole)</param>
        /// <returns>Thông tin user vừa tạo</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/User/register
        ///     {
        ///        "userName": "QuangHau",
        ///        "email": "quanghau@gmail.com",
        ///        "password": "Abc@123",
        ///        "userRole": "Staff"
        ///     }
        ///     
        /// UserRole có thể là: Admin, Staff, User (mặc định là User)
        /// </remarks>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest data)
        {
            var response = await _userService.CreateNewUserAsync(data);
            return Ok(response);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest data)
        {
            var response = await _userService.HandleUserLoginAsync(data.Email, data.Password);
            return Ok(response);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto data)
        {
            var response = await _userService.RefreshTokenAsync(data.AccessToken, data.RefreshToken);
            return Ok(response);
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        /// <returns>Danh sách user</returns>
        /// <remarks>
        /// **Yêu cầu:** Phải đăng nhập với role **Admin** hoặc **Staff**
        /// 
        /// Cách sử dụng:
        /// 1. Đăng nhập bằng tài khoản Admin hoặc Staff
        /// 2. Copy AccessToken từ response
        /// 3. Click "Authorize" và nhập "Bearer {token}"
        /// 4. Gọi API này
        /// </remarks>
        [HttpGet("users")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsersAsync();
            return Ok(response);
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <param name="userId">ID của user cần cập nhật</param>
        /// <param name="data">Thông tin cần cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        /// <remarks>
        /// **Yêu cầu:** Phải đăng nhập với role **Admin**
        /// 
        /// Sample request:
        ///
        ///     PUT /api/User/users/1
        ///     {
        ///        "userName": "newname",
        ///        "email": "newemail@gmail.com",
        ///        "userRole": 0,
        ///        "isActive": true
        ///     }
        ///     
        /// UserRole enum: 0 = Admin, 1 = Staff, 2 = User
        /// </remarks>
        [HttpPut("users/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequest data)
        {
            var response = await _userService.HandleUpdateUserAsync(userId, data);
            return Ok(response);
        }

        /// <summary>
        /// Xóa người dùng
        /// </summary>
        /// <param name="userId">ID của user cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        /// <remarks>
        /// **Yêu cầu:** Phải đăng nhập với role **Admin**
        /// </remarks>
        [HttpDelete("users/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var response = await _userService.HandleDeleteUserAsync(userId);
            return Ok(response);
        }
    }
}