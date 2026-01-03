using DemoEF.Domain.Entities;

using System.Security.Claims;

namespace DemoEF.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = true);
    }
}