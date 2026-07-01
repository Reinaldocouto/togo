using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Togo.Infrastructure.Persistence;

#nullable disable

namespace Togo.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260701120000_AddUserClinicAccess")]
    public partial class AddUserClinicAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserClinicAccesses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClinicAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClinicAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserClinicAccesses_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(name: "IX_UserClinicAccesses_ClinicId", table: "UserClinicAccesses", column: "ClinicId");
            migrationBuilder.CreateIndex(name: "IX_UserClinicAccesses_UserId", table: "UserClinicAccesses", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_UserClinicAccesses_UserId_ClinicId", table: "UserClinicAccesses", columns: new[] { "UserId", "ClinicId" }, unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserClinicAccesses");
        }
    }
}
