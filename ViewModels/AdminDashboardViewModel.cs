namespace ITSL_Administration.ViewModels
{
    // ViewModels/AdminDashboardViewModel.cs
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int DonorCount { get; set; }
        public decimal TotalDonations { get; set; }
        public int ParticipantCount { get; set; }
        public int LecturerCount { get; set; }
        public int TutorCount { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int NewUsersThisWeek { get; set; }
    }
}
