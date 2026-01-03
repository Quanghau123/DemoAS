using DemoEF.Domain.Entities;

using System.Security.Claims;

namespace DemoEF.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, IEnumerable<string> permissions);
        ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = true);
    }
}