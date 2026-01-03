using DemoEF.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetPermissionsByUserAsync(int userId)
    {
        var role = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.UserRole)
            .FirstAsync();

        return await _context.RolePermissions
            .Where(rp => rp.Role == role)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();
    }
}
