using DemoEF.Application.DTOs.Auth;
using DemoEF.Domain.Entities;

namespace DemoEF.Application.Interfaces
{
    public interface IAuthTokenService
    {
        Task<LoginResponseDto> IssueTokenAsync(User user);
    }
}
