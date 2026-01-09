using DemoEF.Application.DTOs.Auth;

namespace DemoEF.Application.Interfaces
{
    public interface IOAuthService
    {
        Task<LoginResponseDto> LoginAsync(string provider, string code);
    }
}
