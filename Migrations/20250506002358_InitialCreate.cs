using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSL_Administration.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminID);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    ModuleID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModuleCredits = table.Column<int>(type: "int", nullable: false),
                    ModuleDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.ModuleID);
                });

            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    DonorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    AmountDonated = table.Column<double>(type: "float", nullable: false),
                    IsActiveVolunteer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.DonorID);
                });

            migrationBuilder.CreateTable(
                name: "Lecturers",
                columns: table => new
                {
                    LecturerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturers", x => x.LecturerID);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantID);
                });

            migrationBuilder.CreateTable(
                name: "Tutors",
                columns: table => new
                {
                    TutorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Campus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRegisteredForITSL = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tutors", x => x.TutorID);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    EnrollmentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParticipantID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TutorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LecturerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Grade = table.Column<double>(type: "float", nullable: true),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.EnrollmentID);
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "Courses",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Lecturers_LecturerID",
                        column: x => x.LecturerID,
                        principalTable: "Lecturers",
                        principalColumn: "LecturerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Participants_ParticipantID",
                        column: x => x.ParticipantID,
                        principalTable: "Participants",
                        principalColumn: "ParticipantID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Tutors_TutorID",
                        column: x => x.TutorID,
                        principalTable: "Tutors",
                        principalColumn: "TutorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_LecturerID",
                table: "Enrollments",
                column: "LecturerID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ModuleID",
                table: "Enrollments",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ParticipantID",
                table: "Enrollments",
                column: "ParticipantID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TutorID",
                table: "Enrollments",
                column: "TutorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Donors");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Lecturers");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Tutors");
        }
    }
}
