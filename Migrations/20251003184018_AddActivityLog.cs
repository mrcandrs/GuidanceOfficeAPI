using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuidanceOfficeAPI.Migrations
{
    public partial class AddActivityLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    ActivityId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityId = table.Column<long>(type: "bigint", nullable: true),
                    Action = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActorType = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActorId = table.Column<long>(type: "bigint", nullable: true),
                    DetailsJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.ActivityId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Action_CreatedAt",
                table: "ActivityLogs",
                columns: new[] { "Action", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ActorType_ActorId_CreatedAt",
                table: "ActivityLogs",
                columns: new[] { "ActorType", "ActorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_EntityType_EntityId_CreatedAt",
                table: "ActivityLogs",
                columns: new[] { "EntityType", "EntityId", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");
        }
    }
}
