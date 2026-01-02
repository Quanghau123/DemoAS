using Microsoft.EntityFrameworkCore;

using DemoEF.Domain.Entities;

namespace DemoEF.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

// dotnet ef migrations add UpdateRefreshTokenEntity
// dotnet ef database update