using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuidanceOfficeAPI.Migrations
{
    public partial class UpdateGuidancePassModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "GuidanceAppointments",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "GuidanceAppointments",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.CreateIndex(
                name: "IX_GuidancePasses_AppointmentId",
                table: "GuidancePasses",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuidancePasses_GuidanceAppointments_AppointmentId",
                table: "GuidancePasses",
                column: "AppointmentId",
                principalTable: "GuidanceAppointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuidancePasses_GuidanceAppointments_AppointmentId",
                table: "GuidancePasses");

            migrationBuilder.DropIndex(
                name: "IX_GuidancePasses_AppointmentId",
                table: "GuidancePasses");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "GuidanceAppointments",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "pending")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "GuidanceAppointments",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "GETDATE()");
        }
    }
}
