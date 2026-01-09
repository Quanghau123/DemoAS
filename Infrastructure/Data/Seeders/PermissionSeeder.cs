using DemoEF.Common.Authorization;
using DemoEF.Domain.Entities;
using DemoEF.Domain.Enums.User;
using DemoEF.Application.Interfaces;

namespace DemoEF.Infrastructure.Data.Seeders
{
    public class PermissionSeeder : ISeeder
    {
        public async Task SeedAsync(AppDbContext context)
        {
            if (context.Permissions.Any())
                return;

            var permissions = new List<Permission>
            {
                new() { Code = Permissions.User_View,   Name = "View user" },
                new() { Code = Permissions.User_Create, Name = "Create user" },
                new() { Code = Permissions.User_Update, Name = "Update user" },
                new() { Code = Permissions.User_Delete, Name = "Delete user" },
                new() { Code = Permissions.User_Export, Name = "Export user" }
            };

            context.Permissions.AddRange(permissions);
            await context.SaveChangesAsync();

            var rolePermissions = new List<RolePermission>
            {
                new() { Role = UserRole.Admin, PermissionId = permissions[0].Id },
                new() { Role = UserRole.Admin, PermissionId = permissions[1].Id },
                new() { Role = UserRole.Admin, PermissionId = permissions[2].Id },
                new() { Role = UserRole.Admin, PermissionId = permissions[3].Id },
                new() { Role = UserRole.Admin, PermissionId = permissions[4].Id },

                new() { Role = UserRole.Staff, PermissionId = permissions[0].Id },
                new() { Role = UserRole.Staff, PermissionId = permissions[2].Id },

                new() { Role = UserRole.User, PermissionId = permissions[0].Id }
            };

            context.RolePermissions.AddRange(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}
