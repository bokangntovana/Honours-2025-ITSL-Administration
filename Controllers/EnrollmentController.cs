using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public EnrollmentController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Enrollment/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.ParticipantId == user.Id)
                .ToListAsync();

            return View(enrollments);
        }

        // GET: Enrollment/Enroll
        public async Task<IActionResult> Enroll()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        // POST: Enrollment/Enroll/{courseId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(string courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                return NotFound();
            }

            // Check if already enrolled
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.ParticipantId == user.Id && e.CourseId == courseId);

            if (existingEnrollment != null)
            {
                TempData["Message"] = "You are already enrolled in this course.";
                return RedirectToAction(nameof(MyCourses));
            }

            var enrollment = new Enrollment
            {
                ParticipantId = user.Id,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Enrollment request submitted successfully!";
            return RedirectToAction(nameof(MyCourses));
        }

        // GET: Enrollment/CourseParticipants/{courseId}
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> CourseParticipants(string courseId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Participant)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();

            ViewBag.CourseName = enrollments.FirstOrDefault()?.Course?.CourseName;
            return View(enrollments);
        }

        // POST: Enrollment/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Admin,Lecturer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int enrollmentId, string status)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound();
            }

            enrollment.Status = status;
            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Enrollment status updated successfully!";
            return RedirectToAction(nameof(CourseParticipants), new { courseId = enrollment.CourseId });
        }

        // GET: Enrollment/ManageGrades/{courseId}
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> ManageGrades(string courseId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Participant)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId && e.Status == "Approved")
                .OrderBy(e => e.Participant.FullName)
                .ToListAsync();

            ViewBag.CourseName = enrollments.FirstOrDefault()?.Course?.CourseName;
            ViewBag.CourseId = courseId;
            return View(enrollments);
        }

        // POST: Enrollment/UpdateGrades
        [HttpPost]
        [Authorize(Roles = "Admin,Lecturer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGrades(string courseId, List<Enrollment> enrollments)
        {
            if (ModelState.IsValid)
            {
                foreach (var enrollment in enrollments)
                {
                    var existingEnrollment = await _context.Enrollments.FindAsync(enrollment.EnrollmentId);
                    if (existingEnrollment != null)
                    {
                        existingEnrollment.Grade = enrollment.Grade;
                        existingEnrollment.CalculateGradeSymbol();
                        existingEnrollment.GradeDate = DateTime.Now;
                        existingEnrollment.GradeFeedback = enrollment.GradeFeedback;

                        // Automatically set status to Completed if graded
                        if (enrollment.Grade.HasValue && existingEnrollment.Status != "Completed")
                        {
                            existingEnrollment.Status = "Completed";
                        }

                        _context.Update(existingEnrollment);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Grades updated successfully!";
                return RedirectToAction(nameof(ManageGrades), new { courseId });
            }

            TempData["ErrorMessage"] = "There were errors updating grades. Please try again.";
            return RedirectToAction(nameof(ManageGrades), new { courseId });
        }

        // GET: Enrollment/GradeReport/{ParticipantId?}
        [Authorize]
        public async Task<IActionResult> GradeReport(string participantId = null)
        {
            string currentUserId = _userManager.GetUserId(User);
            bool isAdminOrLecturer = User.IsInRole("Admin") || User.IsInRole("Lecturer");

            // Admin/Lecturer can view any Participant's report if Id is provided
            if (!string.IsNullOrEmpty(participantId) && isAdminOrLecturer)
            {
                currentUserId = participantId;
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.ParticipantId == currentUserId && e.Grade.HasValue)
                .OrderByDescending(e => e.GradeDate)
                .ToListAsync();

            var student = await _userManager.FindByIdAsync(currentUserId);
            ViewBag.ParticipantName = student?.FullName;
            ViewBag.IsAdminOrLecturer = isAdminOrLecturer;

            return View(enrollments);
        }


    }
}


