using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddSubmissionAssessments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubmissionAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionValues = table.Column<string>(type: "text", nullable: false),
                    MainPoints = table.Column<decimal>(type: "numeric", nullable: false),
                    BonusPoints = table.Column<decimal>(type: "numeric", nullable: false),
                    PenaltyPoints = table.Column<decimal>(type: "numeric", nullable: false),
                    Multiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    FinalGrade = table.Column<decimal>(type: "numeric", nullable: false),
                    CalculationDetails = table.Column<string>(type: "text", nullable: false),
                    CheckedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionAssessments_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionAssessments_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionAssessments_Users_CheckedByUserId",
                        column: x => x.CheckedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionAssessments_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionAssessments_AssignmentId",
                table: "SubmissionAssessments",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionAssessments_CheckedByUserId",
                table: "SubmissionAssessments",
                column: "CheckedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionAssessments_StudentId",
                table: "SubmissionAssessments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionAssessments_SubmissionId",
                table: "SubmissionAssessments",
                column: "SubmissionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmissionAssessments");
        }
    }
}
