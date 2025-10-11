using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<AssignmentController> _logger;
        private readonly UserManager<User> _userManager;

        public AssignmentController(AppDbContext context, IFileUploadService fileUploadService,
            ILogger<AssignmentController> logger, UserManager<User> userManager)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Assignments/Manage?courseId=123
        [Authorize(Roles = "Lecturer,Tutor,Admin,Participant")]
        public async Task<IActionResult> ManageAssignment(string courseId)
        {
            // Simplified access check - role-based only
            if (!User.IsInRole("Admin") && !User.IsInRole("Lecturer") && !User.IsInRole("Tutor"))
            {
                TempData["ErrorMessage"] = "You do not have permission to manage assignments for this course.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            var course = await _context.Courses
                .Include(c => c.Assignments)
                .ThenInclude(a => a.Files)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Assignments/Create?courseId=123
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> Create(string courseId)
        {
            // Simplified role check
            if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only course lecturers can create assignments.";
                return RedirectToAction("ManageAssignment", new { courseId });
            }

            var assignment = new Assignment { CourseId = courseId };
            return View(assignment);
        }

        // POST: Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> Create(Assignment assignment, IFormFile? instructionFile)
        {
            if (ModelState.IsValid)
            {
                // Simplified role check
                if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "Only course lecturers can create assignments.";
                    return View(assignment);
                }

                // Save assignment
                _context.Add(assignment);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(assignment);
                }

                if (instructionFile != null)
                {
                    var file = await _fileUploadService.UploadFileAsync(
                        instructionFile,
                        FileContentType.AssignmentInstruction,
                        userId
                    );

                    // Associate file with assignment if needed
                    assignment.Files.Add(file);
                    await _context.SaveChangesAsync();
                }

                // If assignment is Quiz → redirect to quiz creation
                if (assignment.AssignmentType == AssignmentType.Quiz)
                {
                    return RedirectToAction("Create", "Quizzes", new { assignmentId = assignment.AssignmentID });
                }

                TempData["SuccessMessage"] = "Assignment created successfully.";
                return RedirectToAction("ManageAssignment", new { courseId = assignment.CourseId });
            }

            return View(assignment);
        }

        // GET: Assignments/Edit/5
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Files)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment == null)
            {
                return NotFound();
            }

            // Simplified role check
            if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only course lecturers can edit assignments.";
                return RedirectToAction("ManageAssignment", new { courseId = assignment.CourseId });
            }

            return View(assignment);
        }

        // POST: Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> Edit(string id, Assignment assignment)
        {
            if (id != assignment.AssignmentID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Simplified role check
                    if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
                    {
                        TempData["ErrorMessage"] = "Only course lecturers can edit assignments.";
                        return View(assignment);
                    }

                    _context.Update(assignment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Assignment updated successfully.";
                    return RedirectToAction("ManageAssignment", new { courseId = assignment.CourseId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Assignments.Any(a => a.AssignmentID == id))
                        return NotFound();
                    throw;
                }
            }
            return View(assignment);
        }

        // GET: Assignments/Delete/5
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment == null) return NotFound();

            // Simplified role check
            if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only course lecturers can delete assignments.";
                return RedirectToAction("ManageAssignment", new { courseId = assignment.CourseId });
            }

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Files)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment != null)
            {
                // Simplified role check
                if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "Only course lecturers can delete assignments.";
                    return RedirectToAction("ManageAssignment", new { courseId = assignment.CourseId });
                }

                // delete files from storage + db
                foreach (var file in assignment.Files)
                {
                    await _fileUploadService.DeleteFileAsync(file.FileId);
                }

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Assignment deleted successfully.";
                return RedirectToAction("ManageAssignment", new { courseId = assignment.CourseId });
            }

            return NotFound();
        }

        // GET: Assignment/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Files)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Participant)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.AssignmentID == id);

            if (assignment == null) return NotFound();

            // Simplified access check
            var currentUserId = _userManager.GetUserId(User);
            var isParticipant = assignment.Submissions?.Any(s => s.ParticipantId == currentUserId) ?? false;

            if (!User.IsInRole("Admin") && !User.IsInRole("Lecturer") && !User.IsInRole("Tutor") && !isParticipant)
            {
                TempData["ErrorMessage"] = "You do not have access to this assignment.";
                return RedirectToAction("Index", "Courses");
            }

            // Explicitly return the correct view
            return View("Details", assignment);
        }

        // GET: Assignment/ManageSubmissions/5
        [Authorize(Roles = "Lecturer,Tutor,Admin")]
        public async Task<IActionResult> ManageSubmissions(string id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment == null) return NotFound();

            // Simplified role-based access check
            if (!User.IsInRole("Admin") && !User.IsInRole("Lecturer") && !User.IsInRole("Tutor"))
            {
                TempData["ErrorMessage"] = "You do not have permission to manage submissions for this assignment.";
                return RedirectToAction("Details", "Assignment", new { id });
            }

            var submissions = await _context.Submissions
                .Include(s => s.Participant)
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == id)
                .ToListAsync();

            ViewBag.AssignmentId = id;
            ViewBag.Assignment = assignment;
            return View(submissions);
        }
    }
}