namespace ITSL_Administration.ViewModels
{
    public class LecturerDashboardViewModel
    {
        // Basic counts
        public int TutorCount { get; set; }
        public int ParticipantCount { get; set; }
        public int CourseCount { get; set; }
        public int TotalAssignments { get; set; }

        // Assignment type breakdown
        public int WrittenAssignmentsCount { get; set; }
        public int ProjectAssignmentsCount { get; set; }
        public int QuizAssignmentsCount { get; set; }
        public int ExaminationAssignmentsCount { get; set; }

        // Submission and grading status
        public int SubmittedAssignmentsCount { get; set; }
        public int GradedParticipantsCount { get; set; }
        public int PendingGradingCount { get; set; }

        // Course-wise assignment counts
        public List<CourseAssignmentCount> CourseAssignmentCounts { get; set; } = new();

        // Additional relevant metrics
        public int RecentSubmissionsCount { get; set; } // Last 7 days
        public int OverdueAssignmentsCount { get; set; }
        public decimal AverageGrade { get; set; }
        public int TotalCourseContent { get; set; }
        public int UpcomingDueAssignments { get; set; }
    }

    public class CourseAssignmentCount
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int AssignmentCount { get; set; }
        public int SubmittedCount { get; set; }
        public int GradedCount { get; set; }
        public int PendingGradingCount { get; set; }
    }
}
