using DemoEF.Application.DTOs.User;

namespace DemoEF.Application.Interfaces
{
    public interface IUserService
    {
        Task<object> CreateNewUserAsync(CreateUserRequest data);
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<object> HandleUpdateUserAsync(int userId, UpdateUserRequest data);
        Task<object> HandleDeleteUserAsync(int userId);
    }
}