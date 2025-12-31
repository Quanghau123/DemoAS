using Microsoft.EntityFrameworkCore;
using DemoEF.Domain.Entities;
using DemoEF.Infrastructure.Data.Seeders;

namespace DemoEF.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.SeedUsers();
        }
    }
}

// dotnet ef migrations add UpdateUser_Add_IsActive_Field
// dotnet ef database update