using ITSL_Administration.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Data
{
    public class AppDbContext: IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        //Entity Sets
        public DbSet<Users> Users { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<CourseContent> CourseContents { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure QuizAnswer relationships
            modelBuilder.Entity<QuizAnswer>(entity =>
            {
                // Relationship with Submission (change to Restrict)
                entity.HasOne(a => a.Submission)
                      .WithMany(s => s.QuizAnswers)
                      .HasForeignKey(a => a.SubmissionId)
                      .OnDelete(DeleteBehavior.ClientCascade);

                // Relationship with QuizQuestion (keep Cascade)
                entity.HasOne(a => a.QuizQuestion)
                      .WithMany(q => q.Answers)
                      .HasForeignKey(a => a.QuizQuestionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship with QuestionOption (for SelectedOption)
                entity.HasOne(a => a.SelectedOption)
                      .WithMany()
                      .HasForeignKey(a => a.SelectedOptionId)
                      .OnDelete(DeleteBehavior.ClientCascade);
            });

            // Configure other relationships
            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.HasOne(q => q.Assignment)
                      .WithMany(a => a.QuizQuestions)
                      .HasForeignKey(q => q.AssignmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasOne(s => s.Assignment)
                      .WithMany(a => a.Submissions)
                      .HasForeignKey(s => s.AssignmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }




    }

}
