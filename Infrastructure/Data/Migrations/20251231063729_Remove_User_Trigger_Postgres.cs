using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoEF.Infrastructure.Data.Migrations
{
    public partial class Remove_User_Trigger_Postgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ""trg_user_deactivated"" ON ""Users"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS ""log_user_deactivated"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
