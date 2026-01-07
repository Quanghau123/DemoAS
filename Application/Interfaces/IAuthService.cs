using DemoEF.Application.Auth;
using DemoEF.Application.DTOs.Auth;

namespace DemoEF.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> HandleUserLoginAsync(LoginRequest request);
        Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(int userId);
        Task SendPasswordResetLinkAsync(string email);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}