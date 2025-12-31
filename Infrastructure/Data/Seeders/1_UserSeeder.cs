using DemoEF.Domain.Entities;
using DemoEF.Domain.Enums.User;
using Microsoft.EntityFrameworkCore;

namespace DemoEF.Infrastructure.Data.Seeders
{
    public static class UserSeeder
    {
        public static void SeedUsers(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    Email = "admin@test.com",
                    Password = "123456",
                    UserRole = UserRole.Admin
                },
                new User
                {
                    Id = 2,
                    UserName = "user1",
                    Email = "user1@test.com",
                    Password = "123456",
                    UserRole = UserRole.User
                },
                new User
                {
                    Id = 3,
                    UserName = "user2",
                    Email = "user2@test.com",
                    Password = "123456",
                    UserRole = UserRole.User
                },
                new User
                {
                    Id = 4,
                    UserName = "guest1",
                    Email = "guest1@test.com",
                    Password = "123456",
                    UserRole = UserRole.Guest
                },
                new User
                {
                    Id = 5,
                    UserName = "guest2",
                    Email = "guest2@test.com",
                    Password = "123456",
                    UserRole = UserRole.Guest
                },
                new User
                {
                    Id = 6,
                    UserName = "user3",
                    Email = "user3@test.com",
                    Password = "123456",
                    UserRole = UserRole.User
                },
                new User
                {
                    Id = 7,
                    UserName = "user4",
                    Email = "user4@test.com",
                    Password = "123456",
                    UserRole = UserRole.User
                },
                new User
                {
                    Id = 8,
                    UserName = "user5",
                    Email = "user5@test.com",
                    Password = "123456",
                    UserRole = UserRole.User
                },
                new User
                {
                    Id = 9,
                    UserName = "guest3",
                    Email = "guest3@test.com",
                    Password = "123456",
                    UserRole = UserRole.Guest
                },
                new User
                {
                    Id = 10,
                    UserName = "user6",
                    Email = "user6@test.com",
                    Password = "123456",
                    UserRole = UserRole.User
                }
            );
        }
    }
}
