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
        private readonly IGoogleOAuthClient _googleOAuthClient;
        private readonly IFacebookOAuthClient _facebookOAuthClient;

        public OAuthService(
            AppDbContext context,
            IAuthTokenService authTokenService,
            IGoogleOAuthClient googleOAuthClient,
            IFacebookOAuthClient facebookOAuthClient)
        {
            _context = context;
            _authTokenService = authTokenService;
            _googleOAuthClient = googleOAuthClient;
            _facebookOAuthClient = facebookOAuthClient;
        }
        public async Task<LoginResponseDto> LoginWithGoogleAsync(string code)
        {
            var userInfo = await _googleOAuthClient.GetUserInfoAsync(code);
            return await HandleOAuthUserAsync(userInfo);
        }

        public async Task<LoginResponseDto> LoginWithFacebookAsync(string code)
        {
            var userInfo = await _facebookOAuthClient.GetUserInfoAsync(code);
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