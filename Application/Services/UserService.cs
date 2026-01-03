using DemoEF.Application.DTOs.User;
using DemoEF.Application.Interfaces;
using DemoEF.Domain.Entities;
using DemoEF.Domain.Entities.Requests.User;
using DemoEF.Infrastructure.Data;
using DemoEF.Common.Exceptions;

using Microsoft.EntityFrameworkCore;

namespace DemoEF.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateNewUserAsync(CreateUserRequest data)
        {
            if (string.IsNullOrWhiteSpace(data.UserName))
                throw new ArgumentException("UserName is required.");

            if (string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Password))
                throw new ArgumentException("Email and Password are required.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
            if (existingUser != null)
                throw new ConflictException("User with this email already exists.");

            var user = new User
            {
                UserName = data.UserName.Trim(),
                Email = data.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(data.Password),
                IsActive = true,
                UserRole = string.IsNullOrWhiteSpace(data.UserRole)
                    ? DemoEF.Domain.Enums.User.UserRole.User
                    : Enum.TryParse<DemoEF.Domain.Enums.User.UserRole>(data.UserRole, true, out var parsedRole)
                        ? parsedRole
                        : throw new ArgumentException($"Invalid user role: {data.UserRole}")
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new { UserId = user.Id };
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Role = u.UserRole.ToString(),
                    IsActive = u.IsActive
                })
                .ToListAsync();
        }

        public async Task<object> HandleUpdateUserAsync(int userId, UpdateUserRequest data)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (!string.IsNullOrWhiteSpace(data.UserName))
                user.UserName = data.UserName.Trim();

            if (!string.IsNullOrWhiteSpace(data.Email))
                user.Email = data.Email.Trim();

            if (data.UserRole.HasValue)
                user.UserRole = data.UserRole.Value;

            if (data.IsActive.HasValue)
                user.IsActive = data.IsActive.Value;

            await _context.SaveChangesAsync();

            return new { Message = "User updated successfully." };
        }

        public async Task<object> HandleDeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return new { Message = "User deleted successfully." };
        }
    }
}
