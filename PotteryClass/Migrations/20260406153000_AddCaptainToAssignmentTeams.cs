using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddCaptainToAssignmentTeams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CaptainUserId",
                table: "AssignmentTeams",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTeams_AssignmentId_CaptainUserId",
                table: "AssignmentTeams",
                columns: new[] { "AssignmentId", "CaptainUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTeams_CaptainUserId",
                table: "AssignmentTeams",
                column: "CaptainUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentTeams_Users_CaptainUserId",
                table: "AssignmentTeams",
                column: "CaptainUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentTeams_Users_CaptainUserId",
                table: "AssignmentTeams");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentTeams_AssignmentId_CaptainUserId",
                table: "AssignmentTeams");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentTeams_CaptainUserId",
                table: "AssignmentTeams");

            migrationBuilder.DropColumn(
                name: "CaptainUserId",
                table: "AssignmentTeams");
        }
    }
}
