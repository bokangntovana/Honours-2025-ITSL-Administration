using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")]
    public class AssignmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public AssignmentController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Assignment/CourseAssignments/{courseId}
        public async Task<IActionResult> CourseAssignments(string courseId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.CourseId == courseId)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = _context.Courses.Find(courseId)?.CourseName;
            return View(assignments);
        }

        // GET: Assignment/Create/{courseId}
        public IActionResult Create(string courseId)
        {
            var course = _context.Courses.Find(courseId);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.CourseName = course.CourseName;
            return View(new Assignment { CourseId = courseId });
        }

        // POST: Assignment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,DueDate,MaxPoints,CourseId")] Assignment assignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Assignment created successfully!";
                return RedirectToAction(nameof(CourseAssignments), new { courseId = assignment.CourseId });
            }

            ViewBag.CourseName = _context.Courses.Find(assignment.CourseId)?.CourseName;
            return View(assignment);
        }

        // GET: Assignment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            ViewBag.CourseName = _context.Courses.Find(assignment.CourseId)?.CourseName;
            return View(assignment);
        }

        // POST: Assignment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,Title,Description,DueDate,MaxPoints,CourseId")] Assignment assignment)
        {
            if (id != assignment.AssignmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Assignment updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.AssignmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CourseAssignments), new { courseId = assignment.CourseId });
            }

            ViewBag.CourseName = _context.Courses.Find(assignment.CourseId)?.CourseName;
            return View(assignment);
        }

        // GET: Assignment/CreateQuiz/{courseId}
        public IActionResult CreateQuiz(string courseId)
        {
            var course = _context.Courses.Find(courseId);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.CourseName = course.CourseName;
            return View(new Assignment
            {
                CourseId = courseId,
                AssignmentType = AssignmentType.Quiz
            });
        }

        // POST: Assignment/CreateQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuiz([Bind("Title,Description,DueDate,MaxPoints,CourseId,AssignmentType")] Assignment assignment)
        {
            if (ModelState.IsValid)
            {
                assignment.AssignmentType = AssignmentType.Quiz;
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Quiz created successfully!";
                return RedirectToAction(nameof(QuizController.ManageQuestions), "Quiz", new { assignmentId = assignment.AssignmentId });
                
            }

            ViewBag.CourseName = _context.Courses.Find(assignment.CourseId)?.CourseName;
            return View(assignment);
        }

        // GET: Assignment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            var courseId = assignment.CourseId;
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Assignment deleted successfully!";
            return RedirectToAction(nameof(CourseAssignments), new { courseId });
        }


        // GET: Assignment/Submissions/5
        public async Task<IActionResult> Submissions(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Participant)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignment/GradeSubmission/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(int submissionId, decimal grade, string feedback)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null)
            {
                return NotFound();
            }

            submission.Grade = grade;
            submission.Feedback = feedback;
            submission.GradedDate = DateTime.Now;

            _context.Update(submission);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Submission graded successfully!";

            return RedirectToAction(nameof(Submissions), new { id = submission.AssignmentId });
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.AssignmentId == id);
        }
    }
}
