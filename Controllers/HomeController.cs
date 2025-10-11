using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public HomeController(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                DonorCount = (await _userManager.GetUsersInRoleAsync("Donor")).Count,
                ParticipantCount = (await _userManager.GetUsersInRoleAsync("Participant")).Count,
                LecturerCount = (await _userManager.GetUsersInRoleAsync("Lecturer")).Count,
                TutorCount = (await _userManager.GetUsersInRoleAsync("Tutor")).Count,
                TotalCourses = await _context.Courses.CountAsync(),
                TotalAssignments = await _context.Assignments.CountAsync(),
                TotalDonations = await _context.Donations
                    .Where(d => d.PaymentStatus == "succeeded")
                    .SumAsync(d => d.Amount),
                NewUsersThisWeek = await _userManager.Users
                    .Where(u => u.RegistrationDate >= DateTime.Now.AddDays(-7))
                    .CountAsync()
            };

            return View(model);
        }

        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> LecturerDashboard()
        {
            var model = new LecturerDashboardViewModel
            {
                // User counts
                TutorCount = (await _userManager.GetUsersInRoleAsync("Tutor")).Count,
                ParticipantCount = (await _userManager.GetUsersInRoleAsync("Participant")).Count,
                CourseCount = await _context.Courses.CountAsync(),
                TotalAssignments = await _context.Assignments.CountAsync(),
                TotalCourseContent = await _context.CourseContents.CountAsync(),

                // Assignment type breakdown
                WrittenAssignmentsCount = await _context.Assignments
                    .CountAsync(a => a.AssignmentType == AssignmentType.WrittenAssignment),
                ProjectAssignmentsCount = await _context.Assignments
                    .CountAsync(a => a.AssignmentType == AssignmentType.Project),
                QuizAssignmentsCount = await _context.Assignments
                    .CountAsync(a => a.AssignmentType == AssignmentType.Quiz),
                ExaminationAssignmentsCount = await _context.Assignments
                    .CountAsync(a => a.AssignmentType == AssignmentType.Examination),

                // Submission and grading stats
                SubmittedAssignmentsCount = await _context.Submissions.CountAsync(),
                GradedParticipantsCount = await _context.Grades
                    .Select(g => g.Submission.ParticipantId)
                    .Distinct()
                    .CountAsync(),
                PendingGradingCount = await _context.Submissions
                    .CountAsync(s => s.Grade == null),

                // Recent activity and deadlines
                RecentSubmissionsCount = await _context.Submissions
                    .CountAsync(s => s.DateSubmitted >= DateTime.Now.AddDays(-7)),
                OverdueAssignmentsCount = await _context.Assignments
                    .CountAsync(a => a.DueDate < DateTime.Now &&
                                   !a.Submissions.Any(s => s.DateSubmitted <= a.DueDate)),
                UpcomingDueAssignments = await _context.Assignments
                    .CountAsync(a => a.DueDate >= DateTime.Now && a.DueDate <= DateTime.Now.AddDays(7))
            };

            // Calculate average grade
            var averageGrade = await _context.Grades
                .Where(g => g.AssignmentMark > 0)
                .AverageAsync(g => (double?)g.AssignmentMark) ?? 0;
            model.AverageGrade = (decimal)averageGrade;

            // Course-wise assignment counts
            var courseAssignments = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Grade)
                .Select(c => new CourseAssignmentCount
                {
                    CourseId = c.CourseID,
                    CourseName = c.CourseName ?? "Unnamed Course",
                    CourseCode = c.CourseCode ?? "N/A",
                    AssignmentCount = c.Assignments.Count,
                    SubmittedCount = c.Assignments.SelectMany(a => a.Submissions).Count(),
                    GradedCount = c.Assignments.SelectMany(a => a.Submissions)
                                 .Count(s => s.Grade != null),
                    PendingGradingCount = c.Assignments.SelectMany(a => a.Submissions)
                                        .Count(s => s.Grade == null)
                })
                .Where(c => c.AssignmentCount > 0) // Only show courses with assignments
                .ToListAsync();

            model.CourseAssignmentCounts = courseAssignments;

            return View(model);
        }

        [Authorize(Roles = "Tutor,Admin")]
        public async Task<IActionResult> TutorDashboard()
        {
            var currentUserId = _userManager.GetUserId(User);

            var model = new TutorDashboardViewModel
            {
                // User counts
                ParticipantCount = (await _userManager.GetUsersInRoleAsync("Participant")).Count,
                LecturerCount = (await _userManager.GetUsersInRoleAsync("Lecturer")).Count,
                CourseCount = await _context.Courses.CountAsync(),
                TotalAssignments = await _context.Assignments.CountAsync(),
                CourseContentItems = await _context.CourseContents.CountAsync(),

                // Grading statistics
                AssignmentsToGrade = await _context.Submissions
                    .CountAsync(s => s.Grade == null),
                RecentlyGradedCount = await _context.Grades
                    .CountAsync(g => g.DateRecorded >= DateTime.Now.AddDays(-7)),
                TotalGradedByTutor = await _context.Grades
                    .CountAsync(g => g.MarkedBy == User.Identity!.Name),

                // Submission statistics
                TotalSubmissions = await _context.Submissions.CountAsync(),
                LateSubmissions = await _context.Submissions
                    .Include(s => s.Assignment)
                    .CountAsync(s => s.DateSubmitted > s.Assignment!.DueDate),
                OnTimeSubmissions = await _context.Submissions
                    .Include(s => s.Assignment)
                    .CountAsync(s => s.DateSubmitted <= s.Assignment!.DueDate),

                // Recent activity
                NewSubmissionsToday = await _context.Submissions
                    .CountAsync(s => s.DateSubmitted.Date == DateTime.Today),
                UpcomingDeadlines = await _context.Assignments
                    .CountAsync(a => a.DueDate >= DateTime.Now && a.DueDate <= DateTime.Now.AddDays(7)),
                ActiveCoursesWithAssignments = await _context.Courses
                    .CountAsync(c => c.Assignments.Any())
            };

            // Calculate average grade given by this tutor
            var tutorGrades = await _context.Grades
                .Where(g => g.MarkedBy == User.Identity!.Name && g.AssignmentMark > 0)
                .Select(g => g.AssignmentMark)
                .ToListAsync();

            model.AverageGradeGiven = tutorGrades.Any() ? (decimal)tutorGrades.Average() : 0;

            // Course-wise overview for tutors
            var courseOverviews = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Grade)
                .Where(c => c.Assignments.Any())
                .Select(c => new TutorCourseOverview
                {
                    CourseId = c.CourseID,
                    CourseName = c.CourseName ?? "Unnamed Course",
                    CourseCode = c.CourseCode ?? "N/A",
                    AssignmentCount = c.Assignments.Count,
                    TotalSubmissions = c.Assignments.SelectMany(a => a.Submissions).Count(),
                    SubmissionsToGrade = c.Assignments.SelectMany(a => a.Submissions)
                                     .Count(s => s.Grade == null),
                    GradedSubmissions = c.Assignments.SelectMany(a => a.Submissions)
                                     .Count(s => s.Grade != null),
                    ParticipantCount = c.Assignments.SelectMany(a => a.Submissions)
                                     .Select(s => s.ParticipantId)
                                     .Distinct()
                                     .Count()
                })
                .ToListAsync();

            model.CourseOverviews = courseOverviews;

            return View(model);
        }

        [Authorize(Roles = "Participant,Admin")]
        public async Task<IActionResult> ParticipantDashboard()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Get all assignments across all courses (since participants can access all courses)
            var allAssignments = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions)
                .ToListAsync();

            var participantSubmissions = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(s => s.Grade)
                .Where(s => s.ParticipantId == currentUserId)
                .ToListAsync();

            var participantGrades = await _context.Grades
                .Include(g => g.Submission)
                    .ThenInclude(s => s.Assignment)
                        .ThenInclude(a => a.Course)
                .Where(g => g.Submission.ParticipantId == currentUserId)
                .OrderByDescending(g => g.DateRecorded)
                .Take(5)
                .ToListAsync();

            var model = new ParticipantDashboardViewModel
            {
                // Course and assignment counts
                EnrolledCoursesCount = await _context.Courses.CountAsync(), // All courses are available
                TotalAssignments = allAssignments.Count,
                CompletedAssignments = participantSubmissions.Count,
                PendingAssignments = allAssignments.Count(a =>
                    !participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID)),
                OverdueAssignments = allAssignments.Count(a =>
                    a.DueDate < DateTime.Now &&
                    !participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID && s.DateSubmitted <= a.DueDate)),

                // Grade statistics
                GradedAssignments = participantSubmissions.Count(s => s.Grade != null),
                RecentSubmissions = participantSubmissions.Count(s => s.DateSubmitted >= DateTime.Now.AddDays(-7)),
                UpcomingDeadlines = allAssignments.Count(a =>
                    a.DueDate >= DateTime.Now && a.DueDate <= DateTime.Now.AddDays(14) &&
                    !participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID))
            };

            // Calculate average grade
            var grades = participantSubmissions
                .Where(s => s.Grade != null)
                .Select(s => s.Grade!.AssignmentMark)
                .ToList();

            model.AverageGrade = grades.Any() ? (decimal)grades.Average() : 0;

            // Calculate overall progress
            model.CurrentProgress = allAssignments.Count > 0 ?
                (decimal)participantSubmissions.Count / allAssignments.Count * 100 : 0;

            // Determine performance status
            model.PerformanceStatus = model.AverageGrade switch
            {
                >= 80 => "Excellent",
                >= 70 => "Good",
                >= 60 => "Satisfactory",
                >= 50 => "Needs Improvement",
                _ => "At Risk"
            };

            // Get upcoming deadlines
            var upcomingDeadlines = allAssignments
                .Where(a => a.DueDate >= DateTime.Now &&
                           a.DueDate <= DateTime.Now.AddDays(14) &&
                           !participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID))
                .OrderBy(a => a.DueDate)
                .Take(5)
                .Select(a => new AssignmentDeadline
                {
                    AssignmentId = a.AssignmentID,
                    AssignmentTitle = a.Title ?? "Untitled Assignment",
                    CourseName = a.Course?.CourseName ?? "Unknown Course",
                    DueDate = a.DueDate,
                    DaysUntilDue = (a.DueDate - DateTime.Now).Days,
                    IsSubmitted = participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID)
                })
                .ToList();

            model.NextDeadlines = upcomingDeadlines;

            // Get recent grades
            var recentGrades = participantGrades
                .Select(g => new RecentGrade
                {
                    AssignmentTitle = g.Assignment?.Title ?? "Untitled Assignment",
                    CourseName = g.Assignment?.Course?.CourseName ?? "Unknown Course",
                    Grade = g.AssignmentMark,
                    GradeDate = g.DateRecorded,
                    Feedback = g.GradesFeedback ?? "No feedback provided",
                    HasPassed = g.HasPassed
                })
                .ToList();

            model.RecentGrades = recentGrades;

            // Get course progress breakdown
            var courseProgresses = allAssignments
                .GroupBy(a => a.Course)
                .Where(g => g.Key != null)
                .Select(g => new CourseProgress
                {
                    CourseId = g.Key!.CourseID,
                    CourseName = g.Key.CourseName ?? "Unnamed Course",
                    CourseCode = g.Key.CourseCode ?? "N/A",
                    TotalAssignments = g.Count(),
                    CompletedAssignments = g.Count(a =>
                        participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID)),
                    GradedAssignments = g.Count(a =>
                        participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID && s.Grade != null)),
                    AverageGrade = g.Where(a => participantSubmissions.Any(s => s.AssignmentId == a.AssignmentID && s.Grade != null))
                                  .SelectMany(a => participantSubmissions.Where(s => s.AssignmentId == a.AssignmentID && s.Grade != null)
                                  .Select(s => (decimal)s.Grade!.AssignmentMark))
                                  .DefaultIfEmpty(0)
                                  .Average()
                })
                .ToList();

            foreach (var course in courseProgresses)
            {
                course.ProgressPercentage = course.TotalAssignments > 0 ?
                    (decimal)course.CompletedAssignments / course.TotalAssignments * 100 : 0;

                course.Status = course.ProgressPercentage switch
                {
                    >= 80 => "On Track",
                    >= 50 => "Good Progress",
                    >= 25 => "Needs Attention",
                    _ => "Getting Started"
                };
            }

            model.CourseProgresses = courseProgresses;

            return View(model);
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // You can pass the user's roles to the view, or let the view check User.IsInRole()
                return View();
            }
            else
            {
                // If not authenticated, show public landing page or redirect to login
                return View();
            }
        }
    }
}
