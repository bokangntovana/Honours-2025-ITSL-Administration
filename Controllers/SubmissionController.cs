using ITSL_Administration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ITSL_Administration.Data;
using ITSL_Administration.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class SubmissionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _uploadService;
        private readonly UserManager<User> _userManager;

        public SubmissionController(AppDbContext context, IFileUploadService uploadService, UserManager<User> userManager)
        {
            _context = context;
            _uploadService = uploadService;
            _userManager = userManager;
        }

        // GET: List submissions for an assignment (Staff only)
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> Index(string assignmentId)
        {
            var submissions = await _context.Submissions
                .Include(s => s.Participant)
                .Include(s => s.Grade)
                .Include(s => s.Files)
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();

            ViewBag.AssignmentId = assignmentId;

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == assignmentId);

            if (assignment != null)
            {
                ViewBag.AssignmentTitle = assignment.Title;
                ViewBag.CourseName = assignment.Course?.CourseName;
            }

            return View(submissions);
        }

        // GET: Create submission (Participants only)
        [Authorize(Roles = "Participant,Admin")]
        public async Task<IActionResult> Create(string assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == assignmentId);

            if (assignment == null) return NotFound();

            // Check if user already submitted
            var currentUserId = _userManager.GetUserId(User);
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.ParticipantId == currentUserId);

            if (existingSubmission != null)
            {
                TempData["ErrorMessage"] = "You have already submitted this assignment.";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }

            ViewBag.AssignmentTitle = assignment.Title;
            ViewBag.AssignmentId = assignment.AssignmentID;
            ViewBag.CourseName = assignment.Course?.CourseName;
            ViewBag.DueDate = assignment.DueDate;

            return View();
        }

        // POST: Create submission (Participants only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Participant,Admin")]
        public async Task<IActionResult> Create(string assignmentId, List<IFormFile> files)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == assignmentId);

            if (assignment == null) return NotFound();

            // Check if user already submitted
            var userId = _userManager.GetUserId(User);
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.ParticipantId == userId);

            if (existingSubmission != null)
            {
                TempData["ErrorMessage"] = "You have already submitted this assignment.";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }

            // Validate files
            if (files == null || files.Count == 0 || files.All(f => f.Length == 0))
            {
                TempData["ErrorMessage"] = "Please select at least one file to upload.";
                ViewBag.AssignmentTitle = assignment.Title;
                ViewBag.AssignmentId = assignment.AssignmentID;
                ViewBag.CourseName = assignment.Course?.CourseName;
                ViewBag.DueDate = assignment.DueDate;
                return View();
            }

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                ParticipantId = userId,
                DateSubmitted = DateTime.Now
            };

            try
            {
                // Upload submitted files
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var uploaded = await _uploadService.UploadFileAsync(file, FileContentType.AssignmentSubmission, userId);
                        submission.Files.Add(uploaded);
                    }
                }

                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Assignment submitted successfully!";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your assignment. Please try again.";
                ViewBag.AssignmentTitle = assignment.Title;
                ViewBag.AssignmentId = assignment.AssignmentID;
                ViewBag.CourseName = assignment.Course?.CourseName;
                ViewBag.DueDate = assignment.DueDate;
                return View();
            }
        }

        // GET: Submission Details
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Participant)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(s => s.Grade)
                .Include(s => s.Files)
                .FirstOrDefaultAsync(s => s.SubmissionID == id);

            if (submission == null) return NotFound();

            // Access control
            var currentUserId = _userManager.GetUserId(User);
            var isParticipant = submission.ParticipantId == currentUserId;
            var isStaff = User.IsInRole("Admin") || User.IsInRole("Lecturer") || User.IsInRole("Tutor");

            if (!isParticipant && !isStaff)
            {
                TempData["ErrorMessage"] = "You do not have permission to view this submission.";
                return RedirectToAction("Index", "Courses");
            }

            return View(submission);
        }

        // POST: Delete submission (Staff only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Files)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.SubmissionID == id);

            if (submission == null) return NotFound();

            try
            {
                // Delete associated files from storage
                foreach (var file in submission.Files)
                {
                    await _uploadService.DeleteFileAsync(file.FileId);
                }

                // Delete grade if exists
                if (submission.Grade != null)
                {
                    _context.Grades.Remove(submission.Grade);
                }

                _context.Submissions.Remove(submission);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Submission deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the submission.";
            }

            return RedirectToAction("ManageSubmissions", "Assignment", new { id = submission.AssignmentId });
        }
    }
}