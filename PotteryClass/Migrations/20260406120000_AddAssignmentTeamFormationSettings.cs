using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddAssignmentTeamFormationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CaptainSelectionEndsAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamFormationMode",
                table: "Assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeamFormationEndsAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaptainSelectionEndsAtUtc",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "TeamFormationMode",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "TeamFormationEndsAtUtc",
                table: "Assignments");
        }
    }
}
