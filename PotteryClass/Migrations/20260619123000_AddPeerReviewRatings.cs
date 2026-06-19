using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddPeerReviewRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeerReviewRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeerReviewAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerReviewRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerReviewRatings_AssignmentTeams_ReviewedTeamId",
                        column: x => x.ReviewedTeamId,
                        principalTable: "AssignmentTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeerReviewRatings_AssignmentTeams_ReviewerTeamId",
                        column: x => x.ReviewerTeamId,
                        principalTable: "AssignmentTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeerReviewRatings_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerReviewRatings_PeerReviewAssignments_PeerReviewAssignm~",
                        column: x => x.PeerReviewAssignmentId,
                        principalTable: "PeerReviewAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerReviewRatings_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerReviewRatings_Users_ReviewedUserId",
                        column: x => x.ReviewedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeerReviewRatings_Users_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_AssignmentId",
                table: "PeerReviewRatings",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_PeerReviewAssignmentId",
                table: "PeerReviewRatings",
                column: "PeerReviewAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_PeerReviewAssignmentId_ReviewerUserId_Sub~",
                table: "PeerReviewRatings",
                columns: new[] { "PeerReviewAssignmentId", "ReviewerUserId", "SubmissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_ReviewedTeamId",
                table: "PeerReviewRatings",
                column: "ReviewedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_ReviewedUserId",
                table: "PeerReviewRatings",
                column: "ReviewedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_ReviewerTeamId",
                table: "PeerReviewRatings",
                column: "ReviewerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_ReviewerUserId",
                table: "PeerReviewRatings",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewRatings_SubmissionId",
                table: "PeerReviewRatings",
                column: "SubmissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerReviewRatings");
        }
    }
}
