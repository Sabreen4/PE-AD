using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoFieldsToProgramWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "thumbnail_path",
                schema: "pead_program_service",
                table: "program_weeks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "video_duration",
                schema: "pead_program_service",
                table: "program_weeks",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_name",
                schema: "pead_program_service",
                table: "program_weeks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_path",
                schema: "pead_program_service",
                table: "program_weeks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "video_size",
                schema: "pead_program_service",
                table: "program_weeks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "file_type_id",
                schema: "pead_program_service",
                table: "assignments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "thumbnail_path",
                schema: "pead_program_service",
                table: "program_weeks");

            migrationBuilder.DropColumn(
                name: "video_duration",
                schema: "pead_program_service",
                table: "program_weeks");

            migrationBuilder.DropColumn(
                name: "video_name",
                schema: "pead_program_service",
                table: "program_weeks");

            migrationBuilder.DropColumn(
                name: "video_path",
                schema: "pead_program_service",
                table: "program_weeks");

            migrationBuilder.DropColumn(
                name: "video_size",
                schema: "pead_program_service",
                table: "program_weeks");

            migrationBuilder.AlterColumn<Guid>(
                name: "file_type_id",
                schema: "pead_program_service",
                table: "assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
