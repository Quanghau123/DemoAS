using DemoEF.Infrastructure.Data;

namespace DemoEF.Application.Interfaces
{
    public interface ISeeder
    {
        Task SeedAsync(AppDbContext context);
    }
}
