using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Togo.Infrastructure.Migrations;

public partial class AddClinicalAuditLogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClinicalAuditLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                EntityName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                EntityId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Action = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UserId = table.Column<Guid>(type: "char(36)", nullable: false)
                    .Annotation("MySql:CharSet", "ascii"),
                UserProfile = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                MetadataJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClinicalAuditLogs", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_ClinicalAuditLogs_Action",
            table: "ClinicalAuditLogs",
            column: "Action");

        migrationBuilder.CreateIndex(
            name: "IX_ClinicalAuditLogs_EntityId",
            table: "ClinicalAuditLogs",
            column: "EntityId");

        migrationBuilder.CreateIndex(
            name: "IX_ClinicalAuditLogs_EntityName",
            table: "ClinicalAuditLogs",
            column: "EntityName");

        migrationBuilder.CreateIndex(
            name: "IX_ClinicalAuditLogs_OccurredAt",
            table: "ClinicalAuditLogs",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_ClinicalAuditLogs_UserId",
            table: "ClinicalAuditLogs",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ClinicalAuditLogs");
    }
}
