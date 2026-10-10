using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvaluationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFeedbackSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_attendances_enrollment_id_program_week_id",
                schema: "pead_evaluation_service",
                table: "attendances");

            migrationBuilder.DropColumn(
                name: "feedback_type_id",
                schema: "pead_evaluation_service",
                table: "feedbacks");

            migrationBuilder.AddColumn<Guid>(
                name: "feedback_type_id",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "employee_id",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "feedback_score_adjustment",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "feedback_type_code",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "feedback_type_name",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "i_x_feedbacks_employee_id_weekly_evaluation_id",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                columns: new[] { "employee_id", "weekly_evaluation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_attendances_enrollment_id_program_week_id_attendance_date",
                schema: "pead_evaluation_service",
                table: "attendances",
                columns: new[] { "enrollment_id", "program_week_id", "attendance_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_feedbacks_employee_id_weekly_evaluation_id",
                schema: "pead_evaluation_service",
                table: "feedbacks");

            migrationBuilder.DropIndex(
                name: "i_x_attendances_enrollment_id_program_week_id_attendance_date",
                schema: "pead_evaluation_service",
                table: "attendances");

            migrationBuilder.DropColumn(
                name: "employee_id",
                schema: "pead_evaluation_service",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "feedback_score_adjustment",
                schema: "pead_evaluation_service",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "feedback_type_code",
                schema: "pead_evaluation_service",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "feedback_type_name",
                schema: "pead_evaluation_service",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "feedback_type_id",
                schema: "pead_evaluation_service",
                table: "feedbacks");

            migrationBuilder.AddColumn<long>(
                name: "feedback_type_id",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "i_x_attendances_enrollment_id_program_week_id",
                schema: "pead_evaluation_service",
                table: "attendances",
                columns: new[] { "enrollment_id", "program_week_id" },
                unique: true);
        }
    }
}
