using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProgramService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pead_program_service");

            migrationBuilder.CreateTable(
                name: "training_programs",
                schema: "pead_program_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_id = table.Column<long>(type: "bigint", nullable: false),
                    evaluator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    duration_weeks = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_training_programs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                schema: "pead_program_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    training_program_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_week = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "f_k_enrollments_training_programs_training_program_id",
                        column: x => x.training_program_id,
                        principalSchema: "pead_program_service",
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "program_weeks",
                schema: "pead_program_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    training_program_id = table.Column<long>(type: "bigint", nullable: false),
                    week_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_program_weeks", x => x.id);
                    table.ForeignKey(
                        name: "f_k_program_weeks_training_programs_training_program_id",
                        column: x => x.training_program_id,
                        principalSchema: "pead_program_service",
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignments",
                schema: "pead_program_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    training_program_id = table.Column<long>(type: "bigint", nullable: false),
                    program_week_id = table.Column<long>(type: "bigint", nullable: true),
                    file_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    max_marks = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_assignments", x => x.id);
                    table.ForeignKey(
                        name: "f_k_assignments_program_weeks_program_week_id",
                        column: x => x.program_week_id,
                        principalSchema: "pead_program_service",
                        principalTable: "program_weeks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_assignments_training_programs_training_program_id",
                        column: x => x.training_program_id,
                        principalSchema: "pead_program_service",
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employee_assignments",
                schema: "pead_program_service",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    assignment_id = table.Column<long>(type: "bigint", nullable: false),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    marks = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    reviewer_comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_employee_assignments", x => x.id);
                    table.ForeignKey(
                        name: "f_k_employee_assignments_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalSchema: "pead_program_service",
                        principalTable: "assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_employee_assignments_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalSchema: "pead_program_service",
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_assignments_program_week_id",
                schema: "pead_program_service",
                table: "assignments",
                column: "program_week_id");

            migrationBuilder.CreateIndex(
                name: "i_x_assignments_training_program_id",
                schema: "pead_program_service",
                table: "assignments",
                column: "training_program_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employee_assignments_assignment_id",
                schema: "pead_program_service",
                table: "employee_assignments",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employee_assignments_enrollment_id",
                schema: "pead_program_service",
                table: "employee_assignments",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "i_x_enrollments_training_program_id_employee_id",
                schema: "pead_program_service",
                table: "enrollments",
                columns: new[] { "training_program_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_program_weeks_training_program_id_week_number",
                schema: "pead_program_service",
                table: "program_weeks",
                columns: new[] { "training_program_id", "week_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_training_programs_name",
                schema: "pead_program_service",
                table: "training_programs",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_assignments",
                schema: "pead_program_service");

            migrationBuilder.DropTable(
                name: "assignments",
                schema: "pead_program_service");

            migrationBuilder.DropTable(
                name: "enrollments",
                schema: "pead_program_service");

            migrationBuilder.DropTable(
                name: "program_weeks",
                schema: "pead_program_service");

            migrationBuilder.DropTable(
                name: "training_programs",
                schema: "pead_program_service");
        }
    }
}

