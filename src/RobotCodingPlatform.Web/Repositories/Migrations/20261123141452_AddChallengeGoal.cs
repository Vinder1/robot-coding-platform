using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotCodingPlatform.Web.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GoalX",
                table: "Challenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoalY",
                table: "Challenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoalX",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "GoalY",
                table: "Challenges");
        }
    }
}
