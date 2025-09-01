using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuidanceOfficeAPI.Migrations
{
    public partial class RevisedConsultationFormModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Topic",
                table: "ConsultationForms",
                newName: "Section");

            migrationBuilder.RenameColumn(
                name: "ActionTaken",
                table: "ConsultationForms",
                newName: "SchoolPersonnel");

            migrationBuilder.AddColumn<string>(
                name: "Concerns",
                table: "ConsultationForms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CounselorName",
                table: "ConsultationForms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ConsultationForms",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "GradeYearLevel",
                table: "ConsultationForms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ParentContactNumber",
                table: "ConsultationForms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ParentGuardian",
                table: "ConsultationForms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "ConsultationForms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Time",
                table: "ConsultationForms",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ConsultationForms",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concerns",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "CounselorName",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "GradeYearLevel",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "ParentContactNumber",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "ParentGuardian",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "ConsultationForms");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ConsultationForms");

            migrationBuilder.RenameColumn(
                name: "Section",
                table: "ConsultationForms",
                newName: "Topic");

            migrationBuilder.RenameColumn(
                name: "SchoolPersonnel",
                table: "ConsultationForms",
                newName: "ActionTaken");
        }
    }
}
