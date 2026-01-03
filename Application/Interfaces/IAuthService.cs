using DemoEF.Application.DTOs.Auth;

namespace DemoEF.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> HandleUserLoginAsync(LoginRequest request);
        Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
        Task LogoutAsync(int userId);
    }
}