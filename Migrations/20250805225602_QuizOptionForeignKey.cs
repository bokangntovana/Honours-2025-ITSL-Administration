using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSL_Administration.Migrations
{
    /// <inheritdoc />
    public partial class QuizOptionForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizOptions_QuizQuestions_QuizQuestionQuestionId",
                table: "QuizOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuizOptions_QuizQuestionQuestionId",
                table: "QuizOptions");

            migrationBuilder.DropColumn(
                name: "QuizQuestionQuestionId",
                table: "QuizOptions");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionId",
                table: "QuizOptions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuestionId",
                table: "QuizOptions",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizOptions_QuizQuestions_QuestionId",
                table: "QuizOptions",
                column: "QuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizOptions_QuizQuestions_QuestionId",
                table: "QuizOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuizOptions_QuestionId",
                table: "QuizOptions");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionId",
                table: "QuizOptions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
    }
}
