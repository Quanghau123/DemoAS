using DemoEF.Application.DTOs.Auth;
using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities;
using DemoEF.Domain.Enums.User;
using DemoEF.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace DemoEF.Application.Services
{
    public class OAuthService : IOAuthService
    {
        private readonly AppDbContext _context;
        private readonly IAuthTokenService _authTokenService;
        private readonly Dictionary<string, IOAuthClient> _clients;

        public OAuthService(
            AppDbContext context,
            IAuthTokenService authTokenService,
            IEnumerable<IOAuthClient> clients)
        {
            _context = context;
            _authTokenService = authTokenService;
            _clients = clients.ToDictionary(c => c.ProviderName, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<LoginResponseDto> LoginAsync(string provider, string code)
        {
            if (!_clients.TryGetValue(provider, out var client))
                throw new Exception("Provider not supported");

            var userInfo = await client.GetUserInfoAsync(code);
            return await HandleOAuthUserAsync(userInfo);
        }

        private async Task<LoginResponseDto> HandleOAuthUserAsync(OAuthUserInfoDto userInfo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.AuthProvider == userInfo.Provider &&
                u.ProviderUserId == userInfo.ProviderUserId);

            if (user == null)
            {
                user = new User
                {
                    Email = userInfo.Email,
                    UserName = userInfo.Name,
                    AuthProvider = userInfo.Provider,
                    ProviderUserId = userInfo.ProviderUserId,
                    IsActive = true,
                    UserRole = UserRole.User
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return await _authTokenService.IssueTokenAsync(user);
        }
    }
}
