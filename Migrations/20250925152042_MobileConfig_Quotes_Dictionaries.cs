using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuidanceOfficeAPI.Migrations
{
    public partial class MobileConfig_Quotes_Dictionaries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictionaryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Group = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryItems", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MobileConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<int>(type: "int", nullable: false),
                    MoodCooldownHours = table.Column<int>(type: "int", nullable: false),
                    StudentNumberRegex = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneRegex = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordRegex = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxSiblings = table.Column<int>(type: "int", nullable: false),
                    MaxWorkExperience = table.Column<int>(type: "int", nullable: false),
                    NotificationCooldownMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileConfigs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "DictionaryItems",
                columns: new[] { "Id", "Group", "IsActive", "Value" },
                values: new object[,]
                {
                    { 1, "gradeYears", true, "Grade 11" },
                    { 2, "gradeYears", true, "Grade 12" },
                    { 3, "gradeYears", true, "1st Year" },
                    { 10, "genders", true, "Male" },
                    { 11, "genders", true, "Female" },
                    { 20, "academicLevels", true, "Junior High" },
                    { 21, "academicLevels", true, "Senior High" },
                    { 30, "referredBy", true, "Student" },
                    { 31, "referredBy", true, "Parent" },
                    { 40, "areasOfConcern", true, "Academic" },
                    { 41, "areasOfConcern", true, "Behavioral" },
                    { 50, "actionRequested", true, "Counseling" },
                    { 51, "actionRequested", true, "Classroom Observation" },
                    { 60, "referralPriorities", true, "Emergency" },
                    { 61, "referralPriorities", true, "ASAP" },
                    { 62, "referralPriorities", true, "Before Date" },
                    { 70, "moodLevels", true, "MILD" },
                    { 71, "moodLevels", true, "MODERATE" },
                    { 72, "moodLevels", true, "HIGH" }
                });

            migrationBuilder.InsertData(
                table: "MobileConfigs",
                columns: new[] { "Id", "MaxSiblings", "MaxWorkExperience", "MoodCooldownHours", "NotificationCooldownMs", "PasswordRegex", "PhoneRegex", "StudentNumberRegex", "Version" },
                values: new object[] { 1, 5, 5, 24, 10000, "^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{6,}$", "^\\d{11}$", "^\\d{11}$", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryItems_Group_Value",
                table: "DictionaryItems",
                columns: new[] { "Group", "Value" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictionaryItems");

            migrationBuilder.DropTable(
                name: "MobileConfigs");

            migrationBuilder.DropTable(
                name: "Quotes");
        }
    }
}
