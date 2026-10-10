using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvaluationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWeeklyEvaluationUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_weekly_evaluations_enrollment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations");

            migrationBuilder.DropIndex(
                name: "i_x_weekly_evaluations_program_week_id_employee_assignment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations");

            migrationBuilder.CreateIndex(
                name: "i_x_weekly_evaluations_enrollment_id_program_week_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations",
                columns: new[] { "enrollment_id", "program_week_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_weekly_evaluations_enrollment_id_program_week_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations");

            migrationBuilder.CreateIndex(
                name: "i_x_weekly_evaluations_enrollment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "i_x_weekly_evaluations_program_week_id_employee_assignment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations",
                columns: new[] { "program_week_id", "employee_assignment_id" },
                unique: true);
        }
    }
}
