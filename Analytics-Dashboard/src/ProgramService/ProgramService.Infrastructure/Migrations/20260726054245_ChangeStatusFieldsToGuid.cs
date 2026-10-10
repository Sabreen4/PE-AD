using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStatusFieldsToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TrainingPrograms
            migrationBuilder.Sql("ALTER TABLE pead_program_service.training_programs ALTER COLUMN status_id DROP NOT NULL");
            migrationBuilder.Sql("UPDATE pead_program_service.training_programs SET status_id = NULL WHERE status_id IS NOT NULL");
            migrationBuilder.Sql("ALTER TABLE pead_program_service.training_programs ALTER COLUMN status_id TYPE uuid USING NULL::uuid");

            // Enrollments
            migrationBuilder.Sql("ALTER TABLE pead_program_service.enrollments ALTER COLUMN status_id DROP NOT NULL");
            migrationBuilder.Sql("UPDATE pead_program_service.enrollments SET status_id = NULL WHERE status_id IS NOT NULL");
            migrationBuilder.Sql("ALTER TABLE pead_program_service.enrollments ALTER COLUMN status_id TYPE uuid USING NULL::uuid");

            // EmployeeAssignments
            migrationBuilder.Sql("ALTER TABLE pead_program_service.employee_assignments ALTER COLUMN status_id DROP NOT NULL");
            migrationBuilder.Sql("UPDATE pead_program_service.employee_assignments SET status_id = NULL WHERE status_id IS NOT NULL");
            migrationBuilder.Sql("ALTER TABLE pead_program_service.employee_assignments ALTER COLUMN status_id TYPE uuid USING NULL::uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // TrainingPrograms
            migrationBuilder.Sql("UPDATE pead_program_service.training_programs SET status_id = '0' WHERE status_id IS NULL");
            migrationBuilder.Sql("ALTER TABLE pead_program_service.training_programs ALTER COLUMN status_id TYPE bigint USING NULL::bigint");

            // Enrollments
            migrationBuilder.Sql("UPDATE pead_program_service.enrollments SET status_id = '0' WHERE status_id IS NULL");
            migrationBuilder.Sql("ALTER TABLE pead_program_service.enrollments ALTER COLUMN status_id TYPE bigint USING NULL::bigint");

            // EmployeeAssignments
            migrationBuilder.Sql("UPDATE pead_program_service.employee_assignments SET status_id = '0' WHERE status_id IS NULL");
            migrationBuilder.Sql("ALTER TABLE pead_program_service.employee_assignments ALTER COLUMN status_id TYPE bigint USING NULL::bigint");
        }
    }
}
