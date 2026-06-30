using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

[Migration("20260630130000_AddMedicalRecordClinicId")]
public partial class AddMedicalRecordClinicId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ClinicId",
            table: "MedicalRecords",
            type: "bigint",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE MedicalRecords mr
            INNER JOIN Patients p ON p.Id = mr.PatientId
            SET mr.ClinicId = p.ClinicId
            WHERE mr.ClinicId IS NULL;
            """);

        // If any medical record remains without ClinicId after the backfill, the non-null alter below fails intentionally.

        migrationBuilder.AlterColumn<long>(
            name: "ClinicId",
            table: "MedicalRecords",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.CreateIndex(name: "IX_MedicalRecords_ClinicId", table: "MedicalRecords", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_MedicalRecords_ClinicId_PatientId", table: "MedicalRecords", columns: new[] { "ClinicId", "PatientId" });

        migrationBuilder.AddForeignKey(
            name: "FK_MedicalRecords_Clinics_ClinicId",
            table: "MedicalRecords",
            column: "ClinicId",
            principalTable: "Clinics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_MedicalRecords_Clinics_ClinicId", table: "MedicalRecords");
        migrationBuilder.DropIndex(name: "IX_MedicalRecords_ClinicId", table: "MedicalRecords");
        migrationBuilder.DropIndex(name: "IX_MedicalRecords_ClinicId_PatientId", table: "MedicalRecords");
        migrationBuilder.DropColumn(name: "ClinicId", table: "MedicalRecords");
    }
}
