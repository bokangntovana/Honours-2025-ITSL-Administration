using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSL_Administration.Migrations
{
    /// <inheritdoc />
    public partial class ChangedQuizQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "QuizQuestions");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "QuizOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QuizQuestionQuestionId",
                table: "QuizOptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionQuestionId",
                table: "QuizOptions",
                column: "QuizQuestionQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizOptions_QuizQuestions_QuizQuestionQuestionId",
                table: "QuizOptions",
                column: "QuizQuestionQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizOptions_QuizQuestions_QuizQuestionQuestionId",
                table: "QuizOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuizOptions_QuizQuestionQuestionId",
                table: "QuizOptions");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "QuizOptions");

            migrationBuilder.DropColumn(
                name: "QuizQuestionQuestionId",
                table: "QuizOptions");

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
