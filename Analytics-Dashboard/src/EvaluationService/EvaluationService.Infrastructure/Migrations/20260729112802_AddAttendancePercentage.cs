using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvaluationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendancePercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "attendance_percentage",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attendance_percentage",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations");
        }
    }
}
