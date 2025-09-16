using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ITSL_Administration.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(AppDbContext context, IFileUploadService fileUploadService, ILogger<AssignmentsController> logger)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        // GET: Assignments/Manage?courseId=123
        public async Task<IActionResult> ManageAssignment(string courseId)
        {
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
        public IActionResult Create(string courseId)
        {
            var assignment = new Assignment { CourseId = courseId };
            return View(assignment);
        }

        // POST: Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment assignment, IFormFile? instructionFile)
        {
            if (ModelState.IsValid)
            {
                // Save assignment
                _context.Add(assignment);
                await _context.SaveChangesAsync();

                // If file uploaded → save it
                if (instructionFile != null)
                {
                    var file = await _fileUploadService.UploadFileAsync(
                        instructionFile,
                        FileContentType.AssignmentInstruction,
                        User.Identity!.Name! // uploader ID
                    );
                    assignment.Files.Add(file);
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }

                // If assignment is Quiz → redirect to quiz creation
                if (assignment.AssignmentType == AssignmentType.Quiz)
                {
                    return RedirectToAction("Create", "Quizzes", new { assignmentId = assignment.AssignmentID });
                }

                TempData["SuccessMessage"] = "Assignment created successfully.";
                return RedirectToAction("Manage", new { courseId = assignment.CourseId });
            }

            return View(assignment);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Files)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Assignment assignment)
        {
            if (id != assignment.AssignmentID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Assignment updated successfully.";
                    return RedirectToAction("Manage", new { courseId = assignment.CourseId });
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
        public async Task<IActionResult> Delete(string id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Files)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment != null)
            {
                // delete files from storage + db
                foreach (var file in assignment.Files)
                {
                    await _fileUploadService.DeleteFileAsync(file.FileId);
                }

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Assignment deleted successfully.";
                return RedirectToAction("Manage", new { courseId = assignment.CourseId });
            }

            return NotFound();
        }

        // GET: Assignment/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Files)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Participant)
                .FirstOrDefaultAsync(m => m.AssignmentID == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // GET: Assignment/ManageSubmissions/5
        public async Task<IActionResult> ManageSubmissions(string id)
        {
            var submissions = await _context.Submissions
                .Include(s => s.Participant)
                .Include(s => s.Grade)
                .Where(s => s.AssignmentId == id)
                .ToListAsync();

            ViewBag.AssignmentId = id;
            return View(submissions);
        }
    }
}
