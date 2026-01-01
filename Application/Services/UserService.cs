using DemoEF.Application.DTOs.User;
using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities;
using DemoEF.Infrastructure.Data;
using DemoEF.Domain.Entities.Requests.User;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoEF.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHander = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                     new Claim(ClaimTypes.Role, user.UserRole.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"]!)),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHander.CreateToken(tokenDescriptor);
            return tokenHander.WriteToken(token);
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var tokenHander = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;

            try
            {
                var principal = tokenHander.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                }, out validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    throw new SecurityTokenException("Invalid token");

                var dbToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId.ToString() == userIdClaim && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow);

                if (dbToken == null)
                    throw new SecurityTokenException("Refresh token is invalid or expired.");

                var user = await _context.Users.FindAsync(int.Parse(userIdClaim));
                if (user == null)
                    throw new SecurityTokenException("User not found.");
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
                var newTokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                        new Claim(ClaimTypes.Role, user.UserRole.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"]!)),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var newAccessToken = tokenHander.CreateToken(newTokenDescriptor);
                var jwt = tokenHander.WriteToken(newAccessToken);

                dbToken.IsRevoked = true;
                var newRefreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(newRefreshToken);
                await _context.SaveChangesAsync();

                var dto = new TokenResponseDto
                {
                    AccessToken = jwt,
                    RefreshToken = newRefreshToken.Token
                };

                return dto;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token", ex);
            }
        }

        public async Task<object> CreateNewUserAsync(CreateUserRequest data)
        {
            if (data.UserName == null)
            {
                throw new Exception("UserName is required.");
            }
            if (data.Email == null || data.Password == null)
            {
                throw new Exception("Email and Password are required.");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists.");
            }

            var user = new User
            {
                UserName = data.UserName!.Trim(),
                Email = data.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(data.Password),
                IsActive = true
            };

            if (string.IsNullOrWhiteSpace(data.UserRole))
            {
                user.UserRole = DemoEF.Domain.Enums.User.UserRole.User;
            }
            else if (!Enum.TryParse<DemoEF.Domain.Enums.User.UserRole>(data.UserRole, true, out var parsedRole))
            {
                throw new ArgumentException($"Invalid user role: {data.UserRole}");
            }
            else
            {
                user.UserRole = parsedRole;
            }
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new { Message = "User created successfully.", UserId = user.Id };
        }

        public async Task<LoginResponseDto> HandleUserLoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new Exception("Invalid email");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                throw new Exception("Invalid password");
            }

            var oldTokens = _context.RefreshTokens.Where(t => t.UserId == user.Id && (t.IsRevoked || t.ExpiresAt < DateTime.UtcNow));
            _context.RefreshTokens.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();
            var accessToken = GenerateAccessToken(user);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            var dto = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                User = new LoginUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Role = user.UserRole.ToString(),
                    IsActive = user.IsActive
                }
            };
            return dto;
        }

        public async Task<object> HandleUserLogoutAsync(int userId)
        {
            var tokens = _context.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked);
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _context.SaveChangesAsync();
            return new { Message = "User logged out successfully." };
        }

        public async Task<object> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    Role = u.UserRole.ToString(),
                    u.IsActive
                })
                .ToListAsync();

            return users;
        }

        public async Task<object> HandleUpdateUserAsync(int userId, UpdateUserRequest data)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (!string.IsNullOrWhiteSpace(data.UserName))
                user.UserName = data.UserName.Trim();

            if (!string.IsNullOrWhiteSpace(data.Email))
                user.Email = data.Email.Trim();

            if (data.UserRole.HasValue)
                user.UserRole = data.UserRole.Value;

            if (data.IsActive.HasValue)
                user.IsActive = data.IsActive.Value;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new { Message = "User updated successfully." };
        }

        public async Task<object> HandleDeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return new { Message = "User deleted successfully." };
        }
    }
}