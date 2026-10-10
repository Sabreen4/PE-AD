using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNotificationTypeIdColumnType : Migration
    {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE pead_notification_service.notifications
            DROP COLUMN notification_type_id;
            """);

        migrationBuilder.AddColumn<Guid>(
            name: "notification_type_id",
            schema: "pead_notification_service",
            table: "notifications",
            type: "uuid",
            nullable: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "notification_type_id",
            schema: "pead_notification_service",
            table: "notifications");

        migrationBuilder.AddColumn<long>(
            name: "notification_type_id",
            schema: "pead_notification_service",
            table: "notifications",
            type: "bigint",
            nullable: false);
    }
    }
}
