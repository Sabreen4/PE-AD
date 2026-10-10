using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetadataService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pead_metadata_service");

            migrationBuilder.CreateTable(
                name: "ref_sets",
                schema: "pead_metadata_service",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ref_sets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ref_terms",
                schema: "pead_metadata_service",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ref_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "set_ref_terms",
                schema: "pead_metadata_service",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ref_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ref_term_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_set_ref_terms", x => x.id);
                    table.ForeignKey(
                        name: "f_k_set_ref_terms_ref_sets_ref_set_id",
                        column: x => x.ref_set_id,
                        principalSchema: "pead_metadata_service",
                        principalTable: "ref_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_set_ref_terms_ref_terms_ref_term_id",
                        column: x => x.ref_term_id,
                        principalSchema: "pead_metadata_service",
                        principalTable: "ref_terms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_ref_sets_name",
                schema: "pead_metadata_service",
                table: "ref_sets",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_ref_terms_name",
                schema: "pead_metadata_service",
                table: "ref_terms",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "i_x_set_ref_terms_ref_set_id_ref_term_id",
                schema: "pead_metadata_service",
                table: "set_ref_terms",
                columns: new[] { "ref_set_id", "ref_term_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_set_ref_terms_ref_term_id",
                schema: "pead_metadata_service",
                table: "set_ref_terms",
                column: "ref_term_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "set_ref_terms",
                schema: "pead_metadata_service");

            migrationBuilder.DropTable(
                name: "ref_sets",
                schema: "pead_metadata_service");

            migrationBuilder.DropTable(
                name: "ref_terms",
                schema: "pead_metadata_service");
        }
    }
}
