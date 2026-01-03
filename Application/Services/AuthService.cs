using DemoEF.Application.DTOs.Auth;
using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities;
using DemoEF.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoEF.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;
        private readonly IPermissionService _permissionService;

        public AuthService(
            AppDbContext context,
            IJwtTokenService jwtTokenService,
            IConfiguration configuration,
            IPermissionService permissionService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _permissionService = permissionService;
        }

        public async Task<LoginResponseDto> HandleUserLoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User is inactive.");

            var oldTokens = _context.RefreshTokens
                .Where(t => t.UserId == user.Id &&
                    (t.IsRevoked || t.ExpiresAt < DateTime.UtcNow));

            _context.RefreshTokens.RemoveRange(oldTokens);

            var permissions = await _permissionService.GetPermissionsByUserAsync(user.Id);
            var accessToken = _jwtTokenService.GenerateAccessToken(user, permissions);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
            };
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = _jwtTokenService.GetPrincipalFromToken(accessToken, false);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new SecurityTokenException("Invalid access token.");

            var dbToken = await _context.RefreshTokens.FirstOrDefaultAsync(x =>
                x.Token == refreshToken &&
                x.UserId.ToString() == userIdClaim &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);

            if (dbToken == null)
                throw new SecurityTokenException("Refresh token is invalid or expired.");

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim));
            if (user == null)
                throw new SecurityTokenException("User not found.");

            dbToken.IsRevoked = true;

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshToken);

            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.UserRole.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpireMinutes"]!)
                ),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);

            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = handler.WriteToken(token),
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task LogoutAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
                token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }
    }
}
