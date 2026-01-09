using DemoEF.Application.DTOs.Auth;

namespace DemoEF.Application.Interfaces
{
    public interface IOAuthClient
    {
        string ProviderName { get; }
        Task<OAuthUserInfoDto> GetUserInfoAsync(string code);
    }
}
