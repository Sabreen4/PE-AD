using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvaluationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentIdToWeeklyEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "enrollment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "i_x_weekly_evaluations_enrollment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations",
                column: "enrollment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_weekly_evaluations_enrollment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations");

            migrationBuilder.DropColumn(
                name: "enrollment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations");
        }
    }
}
