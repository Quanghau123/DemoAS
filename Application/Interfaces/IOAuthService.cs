using DemoEF.Application.DTOs.Auth;

namespace DemoEF.Application.Interfaces
{
    public interface IOAuthService
    {
        Task<LoginResponseDto> LoginWithGoogleAsync(string code);
        Task<LoginResponseDto> LoginWithFacebookAsync(string code);
    }
}