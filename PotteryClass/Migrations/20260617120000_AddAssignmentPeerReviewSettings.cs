using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddAssignmentPeerReviewSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PeerReviewEnabled",
                table: "Assignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeerReviewEndsAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeerReviewPenaltyPercent",
                table: "Assignments",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 20m);

            migrationBuilder.AddColumn<int>(
                name: "PeerReviewRequiredReviewsCount",
                table: "Assignments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeerReviewStartsAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeerReviewEnabled",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "PeerReviewEndsAtUtc",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "PeerReviewPenaltyPercent",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "PeerReviewRequiredReviewsCount",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "PeerReviewStartsAtUtc",
                table: "Assignments");
        }
    }
}
