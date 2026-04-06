using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddAssignmentDraftState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DraftCompletedAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DraftCurrentCaptainUserId",
                table: "Assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DraftStartedAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_DraftCurrentCaptainUserId",
                table: "Assignments",
                column: "DraftCurrentCaptainUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Users_DraftCurrentCaptainUserId",
                table: "Assignments",
                column: "DraftCurrentCaptainUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Users_DraftCurrentCaptainUserId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_DraftCurrentCaptainUserId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "DraftCompletedAtUtc",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "DraftCurrentCaptainUserId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "DraftStartedAtUtc",
                table: "Assignments");
        }
    }
}
