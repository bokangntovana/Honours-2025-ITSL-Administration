namespace ITSL_Administration.ViewModels
{
    public class TutorDashboardViewModel
    {
        // Basic counts
        public int ParticipantCount { get; set; }
        public int LecturerCount { get; set; }
        public int CourseCount { get; set; }
        public int TotalAssignments { get; set; }

        // Grading focus
        public int AssignmentsToGrade { get; set; }
        public int RecentlyGradedCount { get; set; } // Last 7 days
        public int TotalGradedByTutor { get; set; }
        public decimal AverageGradeGiven { get; set; }

        // Submission status
        public int TotalSubmissions { get; set; }
        public int LateSubmissions { get; set; }
        public int OnTimeSubmissions { get; set; }

        // Course content
        public int CourseContentItems { get; set; }
        public int ActiveCoursesWithAssignments { get; set; }

        // Recent activity
        public int NewSubmissionsToday { get; set; }
        public int UpcomingDeadlines { get; set; }

        // Course-wise breakdown for tutors
        public List<TutorCourseOverview> CourseOverviews { get; set; } = new();
    }

    public class TutorCourseOverview
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int AssignmentCount { get; set; }
        public int SubmissionsToGrade { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public int ParticipantCount { get; set; }
    }
}
