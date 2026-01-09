using DemoEF.Application.DTOs.Auth;
using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities;
using DemoEF.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace DemoEF.Application.Services
{
    public class AuthTokenService : IAuthTokenService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPermissionService _permissionService;

        public AuthTokenService(
                AppDbContext context,
                IJwtTokenService jwtTokenService,
                IPermissionService permissionService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _permissionService = permissionService;
        }

        public async Task<LoginResponseDto> IssueTokenAsync(User user)
        {
            var oldTokens = await _context.RefreshTokens
                .Where(x => x.UserId == user.Id && !x.IsRevoked)
                .ToListAsync();

            foreach (var token in oldTokens)
                token.IsRevoked = true;

            var permissions =
                await _permissionService.GetPermissionsByUserAsync(user.Id);

            var accessToken =
                _jwtTokenService.GenerateAccessToken(user, permissions);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }
    }
}
