using ITSL_Administration.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Data
{
    public class ITSLAdminDbContext : DbContext
    {
        public ITSLAdminDbContext(DbContextOptions<ITSLAdminDbContext> options) : base(options)
        {
        }

        // Entity Sets for EF Core
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Participant> Participants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary key for Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasKey(e => e.EnrollmentID); // Changed to use EnrollmentID as primary key

            // Configure relationships for Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.ModuleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Tutor)
                .WithMany(t => t.Enrollments)
                .HasForeignKey(e => e.TutorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Lecturer)
                .WithMany(l => l.Enrollments)
                .HasForeignKey(e => e.LecturerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Add any additional configurations as needed
        }
    }
}
