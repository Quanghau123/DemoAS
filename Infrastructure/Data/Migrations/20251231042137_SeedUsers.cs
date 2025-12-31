using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DemoEF.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "UserName", "UserRole" },
                values: new object[,]
                {
                    { 1, "admin@test.com", "123456", "admin", 0 },
                    { 2, "user1@test.com", "123456", "user1", 1 },
                    { 3, "user2@test.com", "123456", "user2", 1 },
                    { 4, "guest1@test.com", "123456", "guest1", 2 },
                    { 5, "guest2@test.com", "123456", "guest2", 2 },
                    { 6, "user3@test.com", "123456", "user3", 1 },
                    { 7, "user4@test.com", "123456", "user4", 1 },
                    { 8, "user5@test.com", "123456", "user5", 1 },
                    { 9, "guest3@test.com", "123456", "guest3", 2 },
                    { 10, "user6@test.com", "123456", "user6", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
