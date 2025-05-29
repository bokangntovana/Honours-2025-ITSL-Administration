using Microsoft.AspNetCore.Mvc;
using ITSL_Administration.Models;
using Microsoft.EntityFrameworkCore;
using ITSL_Administration.Data;
using Microsoft.Extensions.Logging;

namespace ITSL_Administration.Controllers
{
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(AppDbContext context, ILogger<CoursesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string searchString, string filterField)
        {
            var courses = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(c =>
                    c.CourseName.Contains(searchString) ||
                    c.CourseID.Contains(searchString) ||
                    c.CourseDescription.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(filterField))
            {
                courses = filterField switch
                {
                    "Name" => courses.OrderBy(c => c.CourseName),
                    "Credits" => courses.OrderBy(c => c.CourseCredits),
                    _ => courses
                };
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterField = filterField;

            return View(await courses.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseCode,CourseName,CourseCredits,CourseDescription")] Courses course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(course);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
            }

            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CourseID,CourseCode,CourseName,CourseCredits,CourseDescription")] Courses course)
        {
            if (id != course.CourseID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CourseExists(course.CourseID))
                        return NotFound();
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating course");
                        ModelState.AddModelError("", "The record was modified by another user. Please refresh and try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating course");
                    ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                }
            }
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course != null)
                {
                    _context.Courses.Remove(course);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course");
                ModelState.AddModelError("", "An error occurred while deleting. Please try again.");
                return View("Delete", await _context.Courses.FindAsync(id));
            }
        }

        private bool CourseExists(string id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }
    }
}