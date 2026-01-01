using DemoEF.Application.DTOs.User;
using DemoEF.Domain.Entities.Requests.User;

namespace DemoEF.Application.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponseDto> HandleUserLoginAsync(string email, string password);
        Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<object> CreateNewUserAsync(CreateUserRequest data);
        Task<object> GetAllUsersAsync();
        Task<object> HandleUpdateUserAsync(int userId, UpdateUserRequest data);
        Task<object> HandleDeleteUserAsync(int userId);
    }
}