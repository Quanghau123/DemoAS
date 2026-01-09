using DemoEF.Application.DTOs.Auth;

namespace DemoEF.Application.Interfaces
{
    public interface IFacebookOAuthClient
    {
        Task<OAuthUserInfoDto> GetUserInfoAsync(string code);
    }
}
