using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddCourseRegistrationWindow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationEndsAtUtc",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 31, 23, 59, 59, DateTimeKind.Utc));

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationStartsAtUtc",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationEndsAtUtc",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "RegistrationStartsAtUtc",
                table: "Courses");
        }
    }
}
