using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class AddMedicalRecordAuthorship : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "MedicalRecords",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        migrationBuilder.AddColumn<Guid>(
            name: "CreatedByUserId",
            table: "MedicalRecords",
            type: "char(36)",
            nullable: false,
            defaultValue: Guid.Empty)
            .Annotation("MySql:CharSet", "ascii");

        migrationBuilder.AddColumn<Guid>(
            name: "UpdatedByUserId",
            table: "MedicalRecords",
            type: "char(36)",
            nullable: false,
            defaultValue: Guid.Empty)
            .Annotation("MySql:CharSet", "ascii");

        migrationBuilder.Sql("UPDATE MedicalRecords SET CreatedAt = UpdatedAt;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CreatedAt", table: "MedicalRecords");
        migrationBuilder.DropColumn(name: "CreatedByUserId", table: "MedicalRecords");
        migrationBuilder.DropColumn(name: "UpdatedByUserId", table: "MedicalRecords");
    }
}
