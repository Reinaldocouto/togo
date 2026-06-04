using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class DropMedicalRecordAuthorshipDefaults : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE `MedicalRecords` ALTER COLUMN `CreatedAt` DROP DEFAULT;");
        migrationBuilder.Sql("ALTER TABLE `MedicalRecords` ALTER COLUMN `CreatedByUserId` DROP DEFAULT;");
        migrationBuilder.Sql("ALTER TABLE `MedicalRecords` ALTER COLUMN `UpdatedByUserId` DROP DEFAULT;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE `MedicalRecords` ALTER COLUMN `CreatedAt` SET DEFAULT '1970-01-01 00:00:00.000000';");
        migrationBuilder.Sql("ALTER TABLE `MedicalRecords` ALTER COLUMN `CreatedByUserId` SET DEFAULT '00000000-0000-0000-0000-000000000000';");
        migrationBuilder.Sql("ALTER TABLE `MedicalRecords` ALTER COLUMN `UpdatedByUserId` SET DEFAULT '00000000-0000-0000-0000-000000000000';");
    }
}
