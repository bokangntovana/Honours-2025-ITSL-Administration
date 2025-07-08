using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSL_Administration.Migrations
{
    /// <inheritdoc />
    public partial class GradesFuncAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Grade",
                table: "Enrollments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GradeDate",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeFeedback",
                table: "Enrollments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GradeSymbol",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "GradeDate",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "GradeFeedback",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "GradeSymbol",
                table: "Enrollments");
        }
    }
}
