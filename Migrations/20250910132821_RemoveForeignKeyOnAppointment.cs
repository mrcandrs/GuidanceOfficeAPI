using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuidanceOfficeAPI.Migrations
{
    public partial class RemoveForeignKeyOnAppointment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuidancePasses_GuidanceAppointments_AppointmentId",
                table: "GuidancePasses");

            migrationBuilder.DropIndex(
                name: "IX_GuidancePasses_AppointmentId",
                table: "GuidancePasses");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
