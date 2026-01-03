using DemoEF.Application.DTOs.User;
using DemoEF.Common;

namespace DemoEF.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> HandleUserLoginAsync(string email, string password);
        Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
        Task LogoutAsync(int userId);
    }
}