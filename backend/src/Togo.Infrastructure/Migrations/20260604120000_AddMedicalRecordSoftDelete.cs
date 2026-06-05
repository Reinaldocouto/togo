using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class AddMedicalRecordSoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "MedicalRecords",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAt",
            table: "MedicalRecords",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "DeletedByUserId",
            table: "MedicalRecords",
            type: "char(36)",
            nullable: true)
            .Annotation("MySql:CharSet", "ascii");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsDeleted", table: "MedicalRecords");
        migrationBuilder.DropColumn(name: "DeletedAt", table: "MedicalRecords");
        migrationBuilder.DropColumn(name: "DeletedByUserId", table: "MedicalRecords");
    }
}
