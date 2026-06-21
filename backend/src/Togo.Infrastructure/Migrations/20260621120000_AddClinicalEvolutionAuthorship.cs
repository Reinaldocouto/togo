using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations
{
    public partial class AddClinicalEvolutionAuthorship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ClinicalEvolutions",
                type: "char(36)",
                nullable: false,
                defaultValue: Guid.Empty,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ClinicalEvolutions",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "ClinicalEvolutions",
                type: "char(36)",
                nullable: false,
                defaultValue: Guid.Empty,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ClinicalEvolutions",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedByUserId", table: "ClinicalEvolutions");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "ClinicalEvolutions");
            migrationBuilder.DropColumn(name: "UpdatedByUserId", table: "ClinicalEvolutions");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "ClinicalEvolutions");
        }
    }
}
