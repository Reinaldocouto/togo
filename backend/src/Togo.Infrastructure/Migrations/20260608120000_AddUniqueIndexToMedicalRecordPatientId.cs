using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class AddUniqueIndexToMedicalRecordPatientId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords");

        migrationBuilder.DropIndex(
            name: "IX_MedicalRecords_PatientId",
            table: "MedicalRecords");

        migrationBuilder.CreateIndex(
            name: "IX_MedicalRecords_PatientId",
            table: "MedicalRecords",
            column: "PatientId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords",
            column: "PatientId",
            principalTable: "Patients",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords");

        migrationBuilder.DropIndex(
            name: "IX_MedicalRecords_PatientId",
            table: "MedicalRecords");

        migrationBuilder.CreateIndex(
            name: "IX_MedicalRecords_PatientId",
            table: "MedicalRecords",
            column: "PatientId");

        migrationBuilder.AddForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords",
            column: "PatientId",
            principalTable: "Patients",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
