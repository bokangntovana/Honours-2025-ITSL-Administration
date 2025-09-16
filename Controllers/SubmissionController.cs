using ITSL_Administration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ITSL_Administration.Data;
using ITSL_Administration.Services.Interfaces;

namespace ITSL_Administration.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _uploadService;

        public SubmissionController(AppDbContext context, IFileUploadService uploadService)
        {
            _context = context;
            _uploadService = uploadService;
        }

        // List submissions for an assignment
        public async Task<IActionResult> Index(string assignmentId)
        {
            var submissions = await _context.Submissions
                .Include(s => s.Participant)
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();

            ViewBag.AssignmentId = assignmentId;
            return View(submissions);
        }

        // GET: Create submission
        public async Task<IActionResult> Create(string assignmentId)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return NotFound();

            ViewBag.AssignmentTitle = assignment.Title;
            ViewBag.AssignmentId = assignment.AssignmentID;

            return View();
        }

        // POST: Create submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string assignmentId, List<IFormFile> files)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                ParticipantId = userId,
                DateSubmitted = DateTime.Now
            };

            // Upload submitted files
            foreach (var file in files)
            {
                // Replace this line:
                // var uploaded = await _uploadService.UploadFile(file, FileContentType.AssignmentSubmission);

                // With the following, using the correct method signature from IFileUploadService:
                var uploaded = await _uploadService.UploadFileAsync(file, FileContentType.AssignmentSubmission, userId);
                submission.Files.Add(uploaded);
            }

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { assignmentId });
        }

        // Lecturer/Tutor can delete submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null) return NotFound();

            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { assignmentId = submission.AssignmentId });
        }
    }
}
