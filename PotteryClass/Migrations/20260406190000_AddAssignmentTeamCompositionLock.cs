using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddAssignmentTeamCompositionLock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TeamCompositionLockedAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamCompositionLockedAtUtc",
                table: "Assignments");
        }
    }
}
