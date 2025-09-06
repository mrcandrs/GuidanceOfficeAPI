using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuidanceOfficeAPI.Migrations
{
    public partial class AddedUpdatedAtGuidanceNoteTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GuidanceNotes",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GuidanceNotes");
        }
    }
}
