using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using DemoEF.Application.Interfaces;
using DemoEF.Application.DTOs.User;
using DemoEF.Common;

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
        public async Task<IActionResult> Register([FromBody] CreateUserRequest data)
        {
            var result = await _userService.CreateNewUserAsync(data);
            return Ok(new ApiResponse<object>(true, "User created successfully.", result));
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new ApiResponse<object>(true, "Get users successfully.", users));
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
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequest data)
        {
            var result = await _userService.HandleUpdateUserAsync(userId, data);
            return Ok(new ApiResponse<object>(true, "Update success.", result));
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
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var result = await _userService.HandleDeleteUserAsync(userId);
            return Ok(new ApiResponse<object>(true, "Delete success.", result));
        }
    }
}