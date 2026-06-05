using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class ReviewClinicalCascadeDeleteBehavior : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Attendances_Patients_PatientId",
            table: "Attendances");

        migrationBuilder.DropForeignKey(
            name: "FK_ClinicalEvolutions_Attendances_AttendanceId",
            table: "ClinicalEvolutions");

        migrationBuilder.DropForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords");

        migrationBuilder.DropForeignKey(
            name: "FK_Prescriptions_Attendances_AttendanceId",
            table: "Prescriptions");

        migrationBuilder.AddForeignKey(
            name: "FK_Attendances_Patients_PatientId",
            table: "Attendances",
            column: "PatientId",
            principalTable: "Patients",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ClinicalEvolutions_Attendances_AttendanceId",
            table: "ClinicalEvolutions",
            column: "AttendanceId",
            principalTable: "Attendances",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords",
            column: "PatientId",
            principalTable: "Patients",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Prescriptions_Attendances_AttendanceId",
            table: "Prescriptions",
            column: "AttendanceId",
            principalTable: "Attendances",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Attendances_Patients_PatientId",
            table: "Attendances");

        migrationBuilder.DropForeignKey(
            name: "FK_ClinicalEvolutions_Attendances_AttendanceId",
            table: "ClinicalEvolutions");

        migrationBuilder.DropForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords");

        migrationBuilder.DropForeignKey(
            name: "FK_Prescriptions_Attendances_AttendanceId",
            table: "Prescriptions");

        migrationBuilder.AddForeignKey(
            name: "FK_Attendances_Patients_PatientId",
            table: "Attendances",
            column: "PatientId",
            principalTable: "Patients",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ClinicalEvolutions_Attendances_AttendanceId",
            table: "ClinicalEvolutions",
            column: "AttendanceId",
            principalTable: "Attendances",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_MedicalRecords_Patients_PatientId",
            table: "MedicalRecords",
            column: "PatientId",
            principalTable: "Patients",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Prescriptions_Attendances_AttendanceId",
            table: "Prescriptions",
            column: "AttendanceId",
            principalTable: "Attendances",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
