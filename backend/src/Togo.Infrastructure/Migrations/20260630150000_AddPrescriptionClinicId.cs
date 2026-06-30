using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

[Migration("20260630150000_AddPrescriptionClinicId")]
public partial class AddPrescriptionClinicId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ClinicId",
            table: "Prescriptions",
            type: "bigint",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE Prescriptions p
            INNER JOIN Attendances a ON a.Id = p.AttendanceId
            SET p.ClinicId = a.ClinicId
            WHERE p.ClinicId IS NULL;
            """);

        // If any prescription is orphaned or points to an attendance without ClinicId,
        // the non-null alter below fails intentionally instead of inventing a compatibility clinic.

        migrationBuilder.AlterColumn<long>(
            name: "ClinicId",
            table: "Prescriptions",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.CreateIndex(name: "IX_Prescriptions_ClinicId", table: "Prescriptions", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_Prescriptions_ClinicId_AttendanceId", table: "Prescriptions", columns: new[] { "ClinicId", "AttendanceId" });
        migrationBuilder.CreateIndex(name: "IX_Prescriptions_ClinicId_IssuedAt", table: "Prescriptions", columns: new[] { "ClinicId", "IssuedAt" });

        migrationBuilder.AddForeignKey(
            name: "FK_Prescriptions_Clinics_ClinicId",
            table: "Prescriptions",
            column: "ClinicId",
            principalTable: "Clinics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Prescriptions_Clinics_ClinicId", table: "Prescriptions");
        migrationBuilder.DropIndex(name: "IX_Prescriptions_ClinicId", table: "Prescriptions");
        migrationBuilder.DropIndex(name: "IX_Prescriptions_ClinicId_AttendanceId", table: "Prescriptions");
        migrationBuilder.DropIndex(name: "IX_Prescriptions_ClinicId_IssuedAt", table: "Prescriptions");
        migrationBuilder.DropColumn(name: "ClinicId", table: "Prescriptions");
    }
}
