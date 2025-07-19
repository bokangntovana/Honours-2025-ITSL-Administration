using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSL_Administration.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuizQuestionId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Assignments_AssignmentId",
                table: "QuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuizQuestionId",
                table: "QuizAnswers",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "QuizQuestionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Assignments_AssignmentId",
                table: "QuizQuestions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuizQuestionId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Assignments_AssignmentId",
                table: "QuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuizQuestionId",
                table: "QuizAnswers",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "QuizQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Assignments_AssignmentId",
                table: "QuizQuestions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "AssignmentId");
        }
    }
}
