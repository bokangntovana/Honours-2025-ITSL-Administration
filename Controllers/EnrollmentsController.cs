using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSL_Administration.Data;
using ITSL_Administration.Models;

namespace ITSL_Administration.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ITSLAdminDbContext _context;

        public EnrollmentsController(ITSLAdminDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var iTSLAdminDbContext = _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Lecturer)
                .Include(e => e.Participant)
                .Include(e => e.Tutor);
            return View(await iTSLAdminDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Lecturer)
                .Include(e => e.Participant)
                .Include(e => e.Tutor)
                .FirstOrDefaultAsync(m => m.EnrollmentID == id); // Changed from ParticipantID to EnrollmentID
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["ModuleID"] = new SelectList(_context.Courses, "ModuleID", "ModuleName"); // Changed to show ModuleName
            ViewData["LecturerID"] = new SelectList(_context.Lecturers, "LecturerID", "Name"); // Changed to show Name
            ViewData["ParticipantID"] = new SelectList(_context.Participants, "ParticipantID", "Name"); // Changed to show Name
            ViewData["TutorID"] = new SelectList(_context.Tutors, "TutorID", "Name"); // Changed to show Name
            return View();
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentID,ParticipantID,ModuleID,TutorID,LecturerID,Grade,IsPassed")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ModuleID"] = new SelectList(_context.Courses, "ModuleID", "ModuleName", enrollment.ModuleID);
            ViewData["LecturerID"] = new SelectList(_context.Lecturers, "LecturerID", "Name", enrollment.LecturerID);
            ViewData["ParticipantID"] = new SelectList(_context.Participants, "ParticipantID", "Name", enrollment.ParticipantID);
            ViewData["TutorID"] = new SelectList(_context.Tutors, "TutorID", "Name", enrollment.TutorID);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["ModuleID"] = new SelectList(_context.Courses, "ModuleID", "ModuleName", enrollment.ModuleID);
            ViewData["LecturerID"] = new SelectList(_context.Lecturers, "LecturerID", "Name", enrollment.LecturerID);
            ViewData["ParticipantID"] = new SelectList(_context.Participants, "ParticipantID", "Name", enrollment.ParticipantID);
            ViewData["TutorID"] = new SelectList(_context.Tutors, "TutorID", "Name", enrollment.TutorID);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("EnrollmentID,ParticipantID,ModuleID,TutorID,LecturerID,Grade,IsPassed")] Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentID) // Changed from ParticipantID to EnrollmentID
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.EnrollmentID)) // Changed from ParticipantID to EnrollmentID
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ModuleID"] = new SelectList(_context.Courses, "ModuleID", "ModuleName", enrollment.ModuleID);
            ViewData["LecturerID"] = new SelectList(_context.Lecturers, "LecturerID", "Name", enrollment.LecturerID);
            ViewData["ParticipantID"] = new SelectList(_context.Participants, "ParticipantID", "Name", enrollment.ParticipantID);
            ViewData["TutorID"] = new SelectList(_context.Tutors, "TutorID", "Name", enrollment.TutorID);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Lecturer)
                .Include(e => e.Participant)
                .Include(e => e.Tutor)
                .FirstOrDefaultAsync(m => m.EnrollmentID == id); // Changed from ParticipantID to EnrollmentID
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(string id)
        {
            return _context.Enrollments.Any(e => e.EnrollmentID == id); 
        }
    }
}
