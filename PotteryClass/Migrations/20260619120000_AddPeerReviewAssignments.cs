using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddPeerReviewAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeerReviewAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerReviewAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerReviewAssignments_AssignmentTeams_ReviewedTeamId",
                        column: x => x.ReviewedTeamId,
                        principalTable: "AssignmentTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerReviewAssignments_AssignmentTeams_ReviewerTeamId",
                        column: x => x.ReviewerTeamId,
                        principalTable: "AssignmentTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerReviewAssignments_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewAssignments_AssignmentId",
                table: "PeerReviewAssignments",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewAssignments_AssignmentId_ReviewerTeamId_ReviewedTeamId",
                table: "PeerReviewAssignments",
                columns: new[] { "AssignmentId", "ReviewerTeamId", "ReviewedTeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewAssignments_ReviewedTeamId",
                table: "PeerReviewAssignments",
                column: "ReviewedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewAssignments_ReviewerTeamId",
                table: "PeerReviewAssignments",
                column: "ReviewerTeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerReviewAssignments");
        }
    }
}
