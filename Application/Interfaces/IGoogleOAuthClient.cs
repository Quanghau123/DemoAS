using DemoEF.Application.DTOs.Auth;

namespace DemoEF.Application.Interfaces
{
    public interface IGoogleOAuthClient
    {
        Task<OAuthUserInfoDto> GetUserInfoAsync(string code);
    }
}