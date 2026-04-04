using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddAssignmentTeamSizeLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxTeamSize",
                table: "Assignments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinTeamSize",
                table: "Assignments",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxTeamSize",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "MinTeamSize",
                table: "Assignments");
        }
    }
}
