using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuidanceOfficeAPI.Migrations
{
    public partial class AddNewGuidanceNoteTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NoteText",
                table: "GuidanceNotes",
                newName: "TertiarySemester");

            migrationBuilder.AddColumn<string>(
                name: "Assessment",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowThroughDate",
                table: "GuidanceNotes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeYearLevelSection",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Interventions",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewDate",
                table: "GuidanceNotes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsAcademic",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBehavioral",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCareer",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClass",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCounselorInitiated",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFollowThroughSession",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFollowUp",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroup",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndividual",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPersonal",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReferral",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSocial",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWalkIn",
                table: "GuidanceNotes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlanOfAction",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PresentingProblem",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Program",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReferralAgencyName",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReferredBy",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SchoolYear",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SeniorHighQuarter",
                table: "GuidanceNotes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeEnded",
                table: "GuidanceNotes",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeStarted",
                table: "GuidanceNotes",
                type: "time(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assessment",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "FollowThroughDate",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "GradeYearLevelSection",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "Interventions",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "InterviewDate",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsAcademic",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsBehavioral",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsCareer",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsClass",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsCounselorInitiated",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsFollowThroughSession",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsFollowUp",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsGroup",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsIndividual",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsPersonal",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsReferral",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsSocial",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "IsWalkIn",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "PlanOfAction",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "PresentingProblem",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "Program",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "ReferralAgencyName",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "ReferredBy",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "SchoolYear",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "SeniorHighQuarter",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "TimeEnded",
                table: "GuidanceNotes");

            migrationBuilder.DropColumn(
                name: "TimeStarted",
                table: "GuidanceNotes");

            migrationBuilder.RenameColumn(
                name: "TertiarySemester",
                table: "GuidanceNotes",
                newName: "NoteText");
        }
    }
}
