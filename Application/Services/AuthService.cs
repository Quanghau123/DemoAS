using DemoEF.Application.DTOs.Auth;
using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities;
using DemoEF.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DemoEF.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPermissionService _permissionService;

        public AuthService(
            AppDbContext context,
            IJwtTokenService jwtTokenService,
            IPermissionService permissionService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _permissionService = permissionService;
        }

        public async Task<LoginResponseDto> HandleUserLoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User is inactive");

            var oldTokens = await _context.RefreshTokens
                .Where(x => x.UserId == user.Id && !x.IsRevoked)
                .ToListAsync();

            foreach (var token in oldTokens)
                token.IsRevoked = true;

            var permissions =
                await _permissionService.GetPermissionsByUserAsync(user.Id);

            var accessToken =
                _jwtTokenService.GenerateAccessToken(user, permissions);

            var refreshToken = CreateRefreshToken(user.Id);

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var dbToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Token == refreshToken &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow);

            if (dbToken == null)
                throw new SecurityTokenException("Invalid refresh token");

            var user = dbToken.User;

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User is inactive");

            dbToken.IsRevoked = true;

            var newRefreshToken = CreateRefreshToken(user.Id);
            await _context.RefreshTokens.AddAsync(newRefreshToken);

            var permissions =
                await _permissionService.GetPermissionsByUserAsync(user.Id);

            var newAccessToken =
                _jwtTokenService.GenerateAccessToken(user, permissions);

            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task LogoutAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
                token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }

        private static RefreshToken CreateRefreshToken(int userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
        }
    }
}
