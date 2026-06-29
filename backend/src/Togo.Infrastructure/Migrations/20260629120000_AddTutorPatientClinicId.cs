using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class AddTutorPatientClinicId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ClinicId",
            table: "Tutors",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "ClinicId",
            table: "Patients",
            type: "bigint",
            nullable: true);

        migrationBuilder.Sql("""
            INSERT INTO Organizations (Name, IsActive, CreatedAt, UpdatedAt)
            SELECT 'Compatibilidade Fase 8.3.1', TRUE, UTC_TIMESTAMP(6), NULL
            WHERE NOT EXISTS (SELECT 1 FROM Clinics)
              AND (EXISTS (SELECT 1 FROM Tutors) OR EXISTS (SELECT 1 FROM Patients));
            """);

        migrationBuilder.Sql("""
            INSERT INTO Clinics (OrganizationId, Name, IsActive, CreatedAt, UpdatedAt)
            SELECT o.Id, 'Clínica de Compatibilidade Fase 8.3.1', TRUE, UTC_TIMESTAMP(6), NULL
            FROM Organizations o
            WHERE NOT EXISTS (SELECT 1 FROM Clinics)
              AND (EXISTS (SELECT 1 FROM Tutors) OR EXISTS (SELECT 1 FROM Patients))
            ORDER BY o.Id
            LIMIT 1;
            """);

        migrationBuilder.Sql("UPDATE Tutors SET ClinicId = (SELECT Id FROM Clinics ORDER BY Id LIMIT 1) WHERE ClinicId IS NULL;");
        migrationBuilder.Sql("UPDATE Patients SET ClinicId = (SELECT Id FROM Clinics ORDER BY Id LIMIT 1) WHERE ClinicId IS NULL;");

        migrationBuilder.AlterColumn<long>(
            name: "ClinicId",
            table: "Tutors",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.AlterColumn<long>(
            name: "ClinicId",
            table: "Patients",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.CreateIndex(name: "IX_Tutors_ClinicId", table: "Tutors", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_Tutors_ClinicId_Document", table: "Tutors", columns: new[] { "ClinicId", "Document" });
        migrationBuilder.CreateIndex(name: "IX_Patients_ClinicId", table: "Patients", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_Patients_ClinicId_Name", table: "Patients", columns: new[] { "ClinicId", "Name" });

        migrationBuilder.AddForeignKey(
            name: "FK_Tutors_Clinics_ClinicId",
            table: "Tutors",
            column: "ClinicId",
            principalTable: "Clinics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Patients_Clinics_ClinicId",
            table: "Patients",
            column: "ClinicId",
            principalTable: "Clinics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Tutors_Clinics_ClinicId", table: "Tutors");
        migrationBuilder.DropForeignKey(name: "FK_Patients_Clinics_ClinicId", table: "Patients");
        migrationBuilder.DropIndex(name: "IX_Tutors_ClinicId", table: "Tutors");
        migrationBuilder.DropIndex(name: "IX_Tutors_ClinicId_Document", table: "Tutors");
        migrationBuilder.DropIndex(name: "IX_Patients_ClinicId", table: "Patients");
        migrationBuilder.DropIndex(name: "IX_Patients_ClinicId_Name", table: "Patients");
        migrationBuilder.DropColumn(name: "ClinicId", table: "Tutors");
        migrationBuilder.DropColumn(name: "ClinicId", table: "Patients");
    }
}
