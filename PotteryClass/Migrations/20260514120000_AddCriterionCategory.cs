using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotteryClass.Migrations
{
    public partial class AddCriterionCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Criteria",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "main");

            migrationBuilder.Sql(
                "UPDATE \"Criteria\" SET \"Category\" = 'multiplier' WHERE lower(\"Type\") = 'multiplier';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Criteria");
        }
    }
}
