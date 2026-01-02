using DemoEF.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace DemoEF.Infrastructure.Data.Seeders
{
    public static class UserDataSeeder
    {
        public static async Task SeedFromJsonAsync(AppDbContext context, string jsonPath)
        {
            if (!File.Exists(jsonPath)) return;

            var json = await File.ReadAllTextAsync(jsonPath);
            var users = JsonSerializer.Deserialize<List<User>>(json);

            if (users == null || users.Count == 0) return;

            foreach (var user in users)
            {
                var existingUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (existingUser == null)
                {
                    context.Users.Add(user);
                }
                else
                {
                    context.Entry(existingUser)
                           .CurrentValues
                           .SetValues(user);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
