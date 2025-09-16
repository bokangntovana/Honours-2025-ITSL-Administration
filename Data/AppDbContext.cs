using ITSL_Administration.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Diagnostics;

namespace ITSL_Administration.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseContent> CourseContents { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Models.EventSchedule> Events { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enrollment Configuration
            modelBuilder.Entity<Enrollment>(e =>
            {
                e.HasKey(x => new { x.UserId, x.CourseId });

                e.HasOne(x => x.User)
                 .WithMany(u => u.Enrollments)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Course)
                 .WithMany(c => c.Enrollments)
                 .HasForeignKey(x => x.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Assignment Configuration
            modelBuilder.Entity<Assignment>(a =>
            {
                a.HasOne(x => x.Course)
                 .WithMany(c => c.Assignments)
                 .HasForeignKey(x => x.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                a.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Assignment_Weight", "Weight >= 0 AND Weight <= 1");
                    t.HasCheckConstraint("CK_Assignment_SetAssignmentMark", "SetAssignmentMark >= 0");
                });
            });

            // Quiz Configuration
            modelBuilder.Entity<Quiz>(q =>
            {
                q.HasOne(x => x.Assignment)
                 .WithOne()
                 .HasForeignKey<Quiz>(x => x.AssignmentId)
                 .OnDelete(DeleteBehavior.Cascade);

                //q.ToTable(t =>
                //{
                //    t.HasCheckConstraint(
                //        "CK_Quiz_AssignmentType",
                //        "EXISTS (SELECT 1 FROM Assignments WHERE AssignmentID = AssignmentId AND AssignmentType = 2)");
                //});
            });

            // Grade Configuration
            modelBuilder.Entity<Grade>(g =>
            {
                g.HasOne(x => x.Submission)
                 .WithOne(s => s.Grade)
                 .HasForeignKey<Grade>(x => x.SubmissionId)
                 .OnDelete(DeleteBehavior.Cascade);

                g.HasOne(x => x.Assignment)
                 .WithMany()
                 .HasForeignKey(x => x.AssignmentId)
                 .OnDelete(DeleteBehavior.Restrict);

                //g.ToTable(t =>
                //{
                //    t.HasCheckConstraint(
                //        "CK_Grade_AssignmentMark",
                //        "AssignmentMark <= (SELECT SetAssignmentMark FROM Assignments WHERE AssignmentID = AssignmentId)");
                //    t.HasCheckConstraint(
                //        "CK_Grade_FinalMark",
                //        "FinalMark >= 0 AND FinalMark <= 100");
                //});
            });

            // QuizOption Configuration
            modelBuilder.Entity<QuizOption>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
