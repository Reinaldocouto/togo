using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

[Migration("20260630140000_AddClinicalEvolutionClinicId")]
public partial class AddClinicalEvolutionClinicId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ClinicId",
            table: "ClinicalEvolutions",
            type: "bigint",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE ClinicalEvolutions ce
            INNER JOIN Attendances a ON a.Id = ce.AttendanceId
            SET ce.ClinicId = a.ClinicId
            WHERE ce.ClinicId IS NULL;
            """);

        // If any clinical evolution is orphaned or points to an attendance without ClinicId,
        // the non-null alter below fails intentionally instead of inventing a compatibility clinic.

        migrationBuilder.AlterColumn<long>(
            name: "ClinicId",
            table: "ClinicalEvolutions",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.CreateIndex(name: "IX_ClinicalEvolutions_ClinicId", table: "ClinicalEvolutions", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_ClinicalEvolutions_ClinicId_AttendanceId", table: "ClinicalEvolutions", columns: new[] { "ClinicId", "AttendanceId" });
        migrationBuilder.CreateIndex(name: "IX_ClinicalEvolutions_ClinicId_RegisteredAt", table: "ClinicalEvolutions", columns: new[] { "ClinicId", "RegisteredAt" });

        migrationBuilder.AddForeignKey(
            name: "FK_ClinicalEvolutions_Clinics_ClinicId",
            table: "ClinicalEvolutions",
            column: "ClinicId",
            principalTable: "Clinics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_ClinicalEvolutions_Clinics_ClinicId", table: "ClinicalEvolutions");
        migrationBuilder.DropIndex(name: "IX_ClinicalEvolutions_ClinicId", table: "ClinicalEvolutions");
        migrationBuilder.DropIndex(name: "IX_ClinicalEvolutions_ClinicId_AttendanceId", table: "ClinicalEvolutions");
        migrationBuilder.DropIndex(name: "IX_ClinicalEvolutions_ClinicId_RegisteredAt", table: "ClinicalEvolutions");
        migrationBuilder.DropColumn(name: "ClinicId", table: "ClinicalEvolutions");
    }
}
