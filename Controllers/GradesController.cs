using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Controllers
{

    public class GradeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PDFReportService _pdfReportService;

        public GradeController(AppDbContext context, PDFReportService pdfReportService)
        {
            _context = context;
            _pdfReportService = pdfReportService;
        }

        // GET: Grade/Create/{submissionId}
        public async Task<IActionResult> Create(string submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Participant)
                .FirstOrDefaultAsync(s => s.SubmissionID == submissionId);

            if (submission == null) return NotFound();

            var grade = new Grade
            {
                SubmissionId = submissionId,
                AssignmentId = submission.AssignmentId,
                MarkedBy = User.Identity?.Name
            };

            ViewBag.Submission = submission;
            return View(grade);
        }

        // POST: Grade/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Grade grade)
        {
            if (ModelState.IsValid)
            {
                var assignment = await _context.Assignments.FindAsync(grade.AssignmentId);
                if (assignment == null) return NotFound();

                // Pass/Fail logic
                grade.HasPassed = grade.AssignmentMark >= 50;
                grade.DateRecorded = DateTime.Now;

                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageSubmissions", "Assignment", new { id = grade.AssignmentId });
            }

            return View(grade);
        }

        // GET: Grade/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var grade = await _context.Grades
                .Include(g => g.Submission)
                .ThenInclude(s => s.Participant)
                .Include(g => g.Assignment)
                .FirstOrDefaultAsync(g => g.GradeID == id);

            if (grade == null) return NotFound();

            return View(grade);
        }

        // POST: Grade/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Grade grade)
        {
            if (id != grade.GradeID)
                return NotFound();

            if (ModelState.IsValid)
            {
                var assignment = await _context.Assignments.FindAsync(grade.AssignmentId);
                if (assignment == null) return NotFound();

                grade.HasPassed = grade.AssignmentMark >= 50;
                grade.DateRecorded = DateTime.Now;

                _context.Update(grade);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageSubmissions", "Assignment", new { id = grade.AssignmentId });
            }

            return View(grade);
        }

        public async Task<IActionResult> ExportParticipantReport(string courseId, string participantId)
        {
            var course = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Grade)
                .FirstOrDefaultAsync(c => c.CourseID== courseId);

            if (course == null) return NotFound();

            var submissions = course.Assignments
                .SelectMany(a => a.Submissions)
                .Where(s => s.ParticipantId == participantId)
                .ToList();

            var student = submissions.FirstOrDefault()?.Participant;
            if (student == null) return NotFound();

            var pdfBytes = _pdfReportService.GenerateStudentReport(student, course, submissions);
            return File(pdfBytes, "application/pdf", $"{student.FullName}_Report_{course.CourseName}.pdf");
        }

        public async Task<IActionResult> ExportGradebookPdf(string courseId)
        {
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
    }
}
