using Microsoft.AspNetCore.Mvc;
using ITSL_Administration.Models;
using Microsoft.EntityFrameworkCore;
using ITSL_Administration.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using ITSL_Administration.ViewModels;

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

        //Upload Logic Below here
        // GET: Courses/UploadContent/5
        public async Task<IActionResult> UploadContent(string id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            var model = new UploadContentViewModel
            {
                CourseId = course.CourseID,
                CourseName = course.CourseName
            };

            return View(model);
        }

        
        [HttpPost]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> UploadContent(string courseId, IFormFile file, string title, string description)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction("ContentList", new { id = courseId });
            }

            try
            {
                // Create upload directory if it doesn't exist
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save to database
                var content = new CourseContent
                {
                    Title = title,
                    Description = description,
                    FilePath = Path.Combine("uploads", fileName),
                    CourseID = courseId,
                    UploadDate = DateTime.Now
                };

                _context.Add(content);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "File uploaded successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                TempData["ErrorMessage"] = "An error occurred while uploading the file.";
            }

            return RedirectToAction("ContentList", new { id = courseId });
        }

        // GET: Courses/ContentList/5
        public async Task<IActionResult> ContentList(string id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Contents)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        public async Task<IActionResult> DownloadContent(int id)
        {
            var content = await _context.CourseContents.FindAsync(id);
            if (content == null)
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", content.FilePath);
            if (!System.IO.File.Exists(path))
                return NotFound();

            // Get the original file extension
            var originalFileName = content.Title + Path.GetExtension(content.FilePath);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(path), originalFileName);
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                {".pdf", "application/pdf"},
                {".doc", "application/msword"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".ppt", "application/vnd.ms-powerpoint"},
                {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {".zip", "application/zip"},
                {".txt", "text/plain"},
                {".jpg", "image/jpeg"},
                {".png", "image/png"}
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }


        private bool CourseExists(string id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }
    }
}