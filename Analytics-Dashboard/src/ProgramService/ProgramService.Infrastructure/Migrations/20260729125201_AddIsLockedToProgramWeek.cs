using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLockedToProgramWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_locked",
                schema: "pead_program_service",
                table: "program_weeks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_locked",
                schema: "pead_program_service",
                table: "program_weeks");
        }
    }
}
