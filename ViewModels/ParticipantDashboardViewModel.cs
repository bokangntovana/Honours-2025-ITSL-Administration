namespace ITSL_Administration.ViewModels
{
    public class ParticipantDashboardViewModel
    {
        // Course and assignment overview
        public int EnrolledCoursesCount { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int OverdueAssignments { get; set; }

        // Grades and performance
        public int GradedAssignments { get; set; }
        public decimal AverageGrade { get; set; }
        public decimal CurrentProgress { get; set; } // Overall course progress
        public string PerformanceStatus { get; set; } = string.Empty; // Excellent, Good, etc.

        // Upcoming deadlines
        public int UpcomingDeadlines { get; set; }
        public List<AssignmentDeadline> NextDeadlines { get; set; } = new();

        // Recent activity
        public int RecentSubmissions { get; set; } // Last 7 days
        public List<RecentGrade> RecentGrades { get; set; } = new();

        // Course progress breakdown
        public List<CourseProgress> CourseProgresses { get; set; } = new();
    }

    public class AssignmentDeadline
    {
        public string AssignmentId { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public bool IsSubmitted { get; set; }
    }

    public class RecentGrade
    {
        public string AssignmentTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public double Grade { get; set; }
        public DateTime GradeDate { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public bool HasPassed { get; set; }
    }

    public class CourseProgress
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int GradedAssignments { get; set; }
        public decimal AverageGrade { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty; // On Track, Needs Attention, etc.
    }
}
