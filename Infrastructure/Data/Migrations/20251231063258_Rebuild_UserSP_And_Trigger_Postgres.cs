using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoEF.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rebuild_UserSP_And_Trigger_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS ""trg_User_Deactivated"" ON ""Users"";
");

            migrationBuilder.Sql(@"
DROP FUNCTION IF EXISTS ""log_user_deactivated"";
");

            migrationBuilder.Sql(@"
DROP PROCEDURE IF EXISTS ""SetUserActiveStatus"";
");

            migrationBuilder.Sql(@"
CREATE OR REPLACE PROCEDURE ""SetUserActiveStatus""(
    IN p_user_id INT,
    IN p_is_active BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE ""Users""
    SET ""IsActive"" = p_is_active
    WHERE ""Id"" = p_user_id;
END;
$$;
");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION ""log_user_deactivated""()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW.""IsActive"" = false AND OLD.""IsActive"" = true THEN
        INSERT INTO ""UserLogs""(
            ""UserId"",
            ""Action"",
            ""ActionDate""
        )
        VALUES (
            NEW.""Id"",
            'Deactivated',
            NOW()
        );
    END IF;

    RETURN NEW;
END;
$$;
");
            migrationBuilder.Sql(@"
CREATE TRIGGER ""trg_user_deactivated""
AFTER UPDATE ON ""Users""
FOR EACH ROW
EXECUTE FUNCTION ""log_user_deactivated""();
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ""trg_user_deactivated"" ON ""Users"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS ""log_user_deactivated"";");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS ""SetUserActiveStatus"";");
        }
    }
}
