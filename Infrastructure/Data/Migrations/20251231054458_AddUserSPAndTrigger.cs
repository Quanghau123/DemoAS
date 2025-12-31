using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoEF.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSPAndTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE SetUserActiveStatus
                    @UserId INT,
                    @IsActive BIT
                AS
                BEGIN
                    UPDATE Users SET IsActive = @IsActive WHERE Id = @UserId
                END
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_User_Deactivated
                ON Users
                AFTER UPDATE
                AS
                BEGIN
                    INSERT INTO UserLogs(UserId, Action, ActionDate)
                    SELECT i.Id, 'Deactivated', GETDATE()
                    FROM inserted i
                    WHERE i.IsActive = 0
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SetUserActiveStatus");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_User_Deactivated");
        }
    }
}
