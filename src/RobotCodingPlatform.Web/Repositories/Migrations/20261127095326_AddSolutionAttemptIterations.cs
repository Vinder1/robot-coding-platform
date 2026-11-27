using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotCodingPlatform.Web.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSolutionAttemptIterations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Iterations",
                table: "SolutionAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolutionAttempts_ChallengeId_Success_Iterations",
                table: "SolutionAttempts",
                columns: new[] { "ChallengeId", "Success", "Iterations" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SolutionAttempts_ChallengeId_Success_Iterations",
                table: "SolutionAttempts");

            migrationBuilder.DropColumn(
                name: "Iterations",
                table: "SolutionAttempts");
        }
    }
}
