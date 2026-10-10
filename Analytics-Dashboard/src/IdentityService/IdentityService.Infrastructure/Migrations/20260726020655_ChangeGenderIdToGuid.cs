using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGenderIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE pead_identity_service.users
                ALTER COLUMN gender_id DROP NOT NULL
                """);

            migrationBuilder.Sql("""
                UPDATE pead_identity_service.users
                SET gender_id = NULL
                WHERE gender_id IS NOT NULL
                """);

            migrationBuilder.Sql("""
                ALTER TABLE pead_identity_service.users
                ALTER COLUMN gender_id TYPE uuid
                USING NULL::uuid
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE pead_identity_service.users
                ALTER COLUMN gender_id TYPE bigint
                USING NULL::bigint
                """);

            migrationBuilder.Sql("""
                ALTER TABLE pead_identity_service.users
                ALTER COLUMN gender_id SET NOT NULL
                """);
        }
    }
}
