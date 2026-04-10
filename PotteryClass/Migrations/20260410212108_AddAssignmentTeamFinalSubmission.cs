using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddAssignmentTeamFinalSubmission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FinalSubmissionId",
                table: "AssignmentTeams",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTeams_FinalSubmissionId",
                table: "AssignmentTeams",
                column: "FinalSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentTeams_Submissions_FinalSubmissionId",
                table: "AssignmentTeams",
                column: "FinalSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentTeams_Submissions_FinalSubmissionId",
                table: "AssignmentTeams");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentTeams_FinalSubmissionId",
                table: "AssignmentTeams");

            migrationBuilder.DropColumn(
                name: "FinalSubmissionId",
                table: "AssignmentTeams");
        }
    }
}
