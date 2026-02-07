using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ITSL_Administration.ViewModels;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class GradesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PDFReportService _pdfReportService;
        private readonly UserManager<User> _userManager;

        public GradesController(AppDbContext context, PDFReportService pdfReportService, UserManager<User> userManager)
        {
            _context = context;
            _pdfReportService = pdfReportService;
            _userManager = userManager;
        }

        // GET: Grade/Create/{submissionId}
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> Create(string submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(s => s.Participant)
                .FirstOrDefaultAsync(s => s.SubmissionID == submissionId);

            if (submission == null) return NotFound();

            // Simplified role-based access check
            if (!User.IsInRole("Admin") && !User.IsInRole("Lecturer") && !User.IsInRole("Tutor"))
            {
                TempData["ErrorMessage"] = "You do not have grading permissions for this course.";
                return RedirectToAction("ManageSubmissions", "Assignment", new { id = submission.AssignmentId });
            }

            var grade = new Grade
            {
                SubmissionId = submissionId,
                AssignmentId = submission.AssignmentId,
                MarkedBy = User.Identity?.Name
            };

            var viewModel = new GradeCreateViewModel
            {
                Grade = grade,
                Submission = submission
            };

            return View(viewModel);
        }

        // POST: Grade/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> Create(GradeCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var assignment = await _context.Assignments
                    .Include(a => a.Course)
                    .FirstOrDefaultAsync(a => a.AssignmentID == viewModel.Grade.AssignmentId);

                if (assignment == null) return NotFound();

                // REMOVE WEIGHT CALCULATION:
                // double finalMark = (viewModel.Grade.AssignmentMark / assignment.SetAssignmentMark) * (assignment.Weight * 100);

                // USE SIMPLE PERCENTAGE CALCULATION:
                double finalMark = (viewModel.Grade.AssignmentMark / assignment.SetAssignmentMark) * 100;

                // Pass/Fail logic (keep as is)
                viewModel.Grade.HasPassed = finalMark >= 50;
                viewModel.Grade.DateRecorded = DateTime.Now;
                viewModel.Grade.FinalMark = finalMark;
                viewModel.Grade.MarkedBy = User.Identity?.Name;

                _context.Grades.Add(viewModel.Grade);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Grade submitted successfully.";
                return RedirectToAction("ManageSubmissions", "Assignment", new { id = viewModel.Grade.AssignmentId });
            }

            return await LoadSubmissionAndReturnView(viewModel);
        }

        // GET: Grade/Edit/{id}
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var grade = await _context.Grades
                .Include(g => g.Submission)
                    .ThenInclude(s => s.Participant)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(g => g.GradeID == id);

            if (grade == null) return NotFound();

            // Simplified role-based access check
            if (!User.IsInRole("Admin") && !User.IsInRole("Lecturer") && !User.IsInRole("Tutor"))
            {
                TempData["ErrorMessage"] = "You do not have grading permissions for this course.";
                return RedirectToAction("ManageSubmissions", "Assignment", new { id = grade.AssignmentId });
            }

            var viewModel = new GradeCreateViewModel
            {
                Grade = grade,
                Submission = grade.Submission
            };

            return View(viewModel);
        }

        // POST: Grade/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> Edit(string id, GradeCreateViewModel viewModel)
        {
            if (id != viewModel.Grade.GradeID)
                return NotFound();

            if (ModelState.IsValid)
            {
                var assignment = await _context.Assignments
                    .Include(a => a.Course)
                    .FirstOrDefaultAsync(a => a.AssignmentID == viewModel.Grade.AssignmentId);

                if (assignment == null) return NotFound();

                // REMOVE WEIGHT CALCULATION:
                // double finalMark = (viewModel.Grade.AssignmentMark / assignment.SetAssignmentMark) * (assignment.Weight * 100);

                // USE SIMPLE PERCENTAGE CALCULATION:
                double finalMark = (viewModel.Grade.AssignmentMark / assignment.SetAssignmentMark) * 100;

                viewModel.Grade.HasPassed = finalMark >= 50;
                viewModel.Grade.FinalMark = finalMark;
                viewModel.Grade.MarkedBy = User.Identity?.Name;
                viewModel.Grade.DateRecorded = DateTime.Now;

                _context.Update(viewModel.Grade);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Grade updated successfully.";
                return RedirectToAction("ManageSubmissions", "Assignment", new { id = viewModel.Grade.AssignmentId });
            }

            return await LoadSubmissionAndReturnView(viewModel);
        }
        // Helper method to load submission data for the view
        private async Task<IActionResult> LoadSubmissionAndReturnView(GradeCreateViewModel viewModel)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(s => s.Participant)
                .FirstOrDefaultAsync(s => s.SubmissionID == viewModel.Grade.SubmissionId);

            if (submission == null) return NotFound();

            viewModel.Submission = submission;
            return View(viewModel);
        }

        // GET: Grade/Details/{id}
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            var grade = await _context.Grades
                .Include(g => g.Submission)
                    .ThenInclude(s => s.Participant)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(g => g.GradeID == id);

            if (grade == null) return NotFound();

            // Simplified access check
            var currentUserId = _userManager.GetUserId(User);
            var isParticipant = grade.Submission?.ParticipantId == currentUserId;

            if (!isParticipant && !User.IsInRole("Admin") && !User.IsInRole("Lecturer") && !User.IsInRole("Tutor"))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this grade.";
                return RedirectToAction("Index", "Courses");
            }

            return View(grade);
        }

        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> ExportParticipantReport(string courseId, string participantId)
        {
            // Simplified access check - rely on authorization attributes
            var course = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Grade)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);

            if (course == null) return NotFound();

            // REMOVED: Participant enrollment verification

            var submissions = course.Assignments
                .SelectMany(a => a.Submissions)
                .Where(s => s.ParticipantId == participantId)
                .ToList();

            var student = await _userManager.FindByIdAsync(participantId);
            if (student == null) return NotFound();

            var pdfBytes = _pdfReportService.GenerateParticipantReport(student, course, submissions);
            return File(pdfBytes, "application/pdf", $"{student.FullName}_Report_{course.CourseName}.pdf");
        }

        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> ExportGradebookPdf(string courseId)
        {
            // Simplified access check - rely on authorization attributes
            var course = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Grade)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Participant)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);

            if (course == null) return NotFound();

            var pdfBytes = _pdfReportService.GenerateGradebookReport(course);
            return File(pdfBytes, "application/pdf", $"{course.CourseName}_Gradebook.pdf");
        }

        // GET: Grade/ParticipantGrades
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> ParticipantGrades()
        {
            var currentUserId = _userManager.GetUserId(User);

            var grades = await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(g => g.Submission)
                .Where(g => g.Submission != null && g.Submission.ParticipantId == currentUserId)
                .OrderBy(g => g.Assignment!.Course!.CourseName)
                .ThenBy(g => g.Assignment!.DueDate)
                .ToListAsync();

            return View(grades);
        }

        // GET: Grade/ManageGrades/{courseId}
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> ManageGrades(string courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Participant)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Grade)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);

            if (course == null) return NotFound();

            // Simplified access check - rely on authorization attributes
            return View(course);
        }

        // GET: Grade/GradingOverview
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> GradingOverview()
        {
            // REMOVED: Enrollment-based course filtering
            // Now show all courses for grading overview
            var courses = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                .ToListAsync();

            return View(courses);
        }
    }
}