using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations
{
    public partial class AddAttendanceAuthorship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Attendances",
                type: "char(36)",
                nullable: false,
                defaultValue: Guid.Empty,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Attendances",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Attendances",
                type: "char(36)",
                nullable: false,
                defaultValue: Guid.Empty,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Attendances",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<Guid>(
                name: "ClosedByUserId",
                table: "Attendances",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CanceledByUserId",
                table: "Attendances",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CanceledAt",
                table: "Attendances",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedByUserId", table: "Attendances");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Attendances");
            migrationBuilder.DropColumn(name: "UpdatedByUserId", table: "Attendances");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Attendances");
            migrationBuilder.DropColumn(name: "ClosedByUserId", table: "Attendances");
            migrationBuilder.DropColumn(name: "CanceledByUserId", table: "Attendances");
            migrationBuilder.DropColumn(name: "CanceledAt", table: "Attendances");
        }
    }
}
