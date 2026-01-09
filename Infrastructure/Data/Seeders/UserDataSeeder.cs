using DemoEF.Domain.Entities;
using DemoEF.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace DemoEF.Infrastructure.Data.Seeders
{
    public class UserDataSeeder : ISeeder
    {
        public async Task SeedAsync(AppDbContext context)
        {
            string jsonPath = "Infrastructure/Data/Seeders/users.json";
            await SeedFromJsonAsync(context, jsonPath);
        }

        private async Task SeedFromJsonAsync(AppDbContext context, string jsonPath)
        {
            if (!File.Exists(jsonPath)) return;

            var json = await File.ReadAllTextAsync(jsonPath);
            var users = JsonSerializer.Deserialize<List<User>>(json);

            if (users != null && users.Count > 0)
            {
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
                        context.Entry(existingUser).CurrentValues.SetValues(user);
                    }
                }

                var userIdsInJson = users.Select(u => u.Id).ToHashSet();
                var usersToDelete = context.Users.Where(u => !userIdsInJson.Contains(u.Id));
                context.Users.RemoveRange(usersToDelete);

                await context.SaveChangesAsync();
            }
        }
    }
}
