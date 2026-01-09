using DemoEF.Application.Interfaces;

namespace DemoEF.Infrastructure.Data.Seeders
{
    public class DatabaseSeeder
    {
        private readonly IEnumerable<ISeeder> _seeders;

        public DatabaseSeeder(IEnumerable<ISeeder> seeders)
        {
            _seeders = seeders;
        }

        public async Task SeedAsync(AppDbContext context)
        {
            foreach (var seeder in _seeders)
            {
                await seeder.SeedAsync(context);
            }
        }
    }
}
