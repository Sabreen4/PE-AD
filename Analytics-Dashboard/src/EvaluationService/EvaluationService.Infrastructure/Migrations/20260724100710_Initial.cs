using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EvaluationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pead_evaluation_service");

            migrationBuilder.CreateTable(
                name: "assessment_evaluations",
                schema: "pead_evaluation_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_assignment_id = table.Column<long>(type: "bigint", nullable: false),
                    score = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_assessment_evaluations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "attendances",
                schema: "pead_evaluation_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    program_week_id = table.Column<long>(type: "bigint", nullable: false),
                    attendance_date = table.Column<DateOnly>(type: "date", nullable: false),
                    attendance_status_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_attendances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leaderboards",
                schema: "pead_evaluation_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    overall_score = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_leaderboards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_evaluations",
                schema: "pead_evaluation_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    program_week_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_assignment_id = table.Column<long>(type: "bigint", nullable: false),
                    attendance_score = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    engagement_score = table.Column<int>(type: "integer", nullable: false),
                    application_score = table.Column<int>(type: "integer", nullable: false),
                    weekly_average = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_weekly_evaluations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                schema: "pead_evaluation_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    weekly_evaluation_id = table.Column<long>(type: "bigint", nullable: false),
                    feedback_type_id = table.Column<long>(type: "bigint", nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_feedbacks", x => x.id);
                    table.ForeignKey(
                        name: "f_k_feedbacks_weekly_evaluations_weekly_evaluation_id",
                        column: x => x.weekly_evaluation_id,
                        principalSchema: "pead_evaluation_service",
                        principalTable: "weekly_evaluations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_assessment_evaluations_employee_assignment_id",
                schema: "pead_evaluation_service",
                table: "assessment_evaluations",
                column: "employee_assignment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_attendances_enrollment_id_program_week_id",
                schema: "pead_evaluation_service",
                table: "attendances",
                columns: new[] { "enrollment_id", "program_week_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_feedbacks_weekly_evaluation_id",
                schema: "pead_evaluation_service",
                table: "feedbacks",
                column: "weekly_evaluation_id");

            migrationBuilder.CreateIndex(
                name: "i_x_leaderboards_enrollment_id",
                schema: "pead_evaluation_service",
                table: "leaderboards",
                column: "enrollment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_weekly_evaluations_program_week_id_employee_assignment_id",
                schema: "pead_evaluation_service",
                table: "weekly_evaluations",
                columns: new[] { "program_week_id", "employee_assignment_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_evaluations",
                schema: "pead_evaluation_service");

            migrationBuilder.DropTable(
                name: "attendances",
                schema: "pead_evaluation_service");

            migrationBuilder.DropTable(
                name: "feedbacks",
                schema: "pead_evaluation_service");

            migrationBuilder.DropTable(
                name: "leaderboards",
                schema: "pead_evaluation_service");

            migrationBuilder.DropTable(
                name: "weekly_evaluations",
                schema: "pead_evaluation_service");
        }
    }
}
