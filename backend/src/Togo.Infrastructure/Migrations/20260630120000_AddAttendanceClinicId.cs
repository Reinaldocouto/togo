using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class AddAttendanceClinicId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ClinicId",
            table: "Attendances",
            type: "bigint",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE Attendances a
            INNER JOIN Patients p ON p.Id = a.PatientId
            SET a.ClinicId = p.ClinicId
            WHERE a.ClinicId IS NULL;
            """);

        // If any attendance remains without ClinicId after the backfill, the non-null alter below fails intentionally.

        migrationBuilder.AlterColumn<long>(
            name: "ClinicId",
            table: "Attendances",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.CreateIndex(name: "IX_Attendances_ClinicId", table: "Attendances", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_Attendances_ClinicId_OpenedAt", table: "Attendances", columns: new[] { "ClinicId", "OpenedAt" });
        migrationBuilder.CreateIndex(name: "IX_Attendances_ClinicId_Status", table: "Attendances", columns: new[] { "ClinicId", "Status" });

        migrationBuilder.AddForeignKey(
            name: "FK_Attendances_Clinics_ClinicId",
            table: "Attendances",
            column: "ClinicId",
            principalTable: "Clinics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Attendances_Clinics_ClinicId", table: "Attendances");
        migrationBuilder.DropIndex(name: "IX_Attendances_ClinicId", table: "Attendances");
        migrationBuilder.DropIndex(name: "IX_Attendances_ClinicId_OpenedAt", table: "Attendances");
        migrationBuilder.DropIndex(name: "IX_Attendances_ClinicId_Status", table: "Attendances");
        migrationBuilder.DropColumn(name: "ClinicId", table: "Attendances");
    }
}
