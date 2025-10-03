using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.Services;
using ITSL_Administration.Services.Interfaces;
using ITSL_Administration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CoursesController> _logger;
        private readonly IFileUploadService _fileUploadService;
        private readonly UserManager<User> _userManager;

        public CoursesController(AppDbContext context,
            ILogger<CoursesController> logger,
             IFileUploadService fileUploadService,
             UserManager<User> userManager)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileUploadService;
            _userManager = userManager;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string searchString, string filterField)
        {
            var courses = _context.Courses.AsQueryable();

            // REMOVED: Enrollment filtering for non-admin users
            // All authenticated users can see all courses now

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
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return NotFound();

            // REMOVED: Enrollment-based access check
            // All authenticated users can view course details now

            return View(course);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("CourseCode,CourseName,CourseCredits,CourseDescription")] Course course)
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, [Bind("CourseID,CourseCode,CourseName,CourseCredits,CourseDescription")] Course course)
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [Authorize]
        public async Task<IActionResult> ContentList(string id)
        {
            // REMOVED: Enrollment-based access check
            // All authenticated users can access content now

            var course = await _context.Courses
                .Include(c => c.Contents)
                .ThenInclude(c => c.File)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // GET: Courses/UploadContent/{id}
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> UploadContent(string id)
        {
            // Simplified role check
            if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only course lecturers can upload content.";
                return RedirectToAction("ContentList", new { id });
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            var vm = new UploadContentViewModel
            {
                CourseId = course.CourseID,
                CourseName = course.CourseName ?? "Unknown Course"
            };

            return View(vm);
        }

        // POST: Courses/UploadContent/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> UploadContent(string id, UploadContentViewModel model)
        {
            _logger.LogInformation($"UploadContent POST called. ID: {id}, ModelState: {ModelState.IsValid}");

            // Simplified role check
            if (!User.IsInRole("Lecturer") && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only course lecturers can upload content.";
                return RedirectToAction("ContentList", new { id });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        _logger.LogWarning($"- {key}: {error.ErrorMessage}");
                    }
                }

                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course != null)
                {
                    model.CourseName = course.CourseName ?? "Unknown Course";
                }
                return View(model);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    ModelState.AddModelError("", "You must be logged in to upload content.");
                    var course = await _context.Courses.FindAsync(model.CourseId);
                    if (course != null) model.CourseName = course.CourseName;
                    return View(model);
                }

                var uploadedFile = await _fileUploadService.UploadFileAsync(
                    model.File, FileContentType.CourseContent, userId);

                var content = new CourseContent
                {
                    Title = model.Title,
                    Description = model.Description,
                    FileId = uploadedFile.FileId,
                    CourseId = model.CourseId
                };

                _context.CourseContents.Add(content);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Content uploaded successfully.";
                return RedirectToAction("ContentList", new { id = model.CourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading course content");
                ModelState.AddModelError("", "An error occurred while uploading. Please try again.");

                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course != null) model.CourseName = course.CourseName;

                return View(model);
            }
        }

        // GET: Courses/DownloadContent/{id}
        [Authorize]
        public async Task<IActionResult> DownloadContent(string id)
        {
            try
            {
                var content = await _context.CourseContents
                    .Include(c => c.File)
                    .Include(c => c.Course)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (content?.File == null)
                    return NotFound();

                // REMOVED: Enrollment-based access check
                // All authenticated users can download content now

                var stream = await _fileUploadService.DownloadFileAsync(content.FileId);

                var contentType = GetContentType(content.File.FileName);
                return File(stream, contentType, content.File.FileName);
            }
            catch (FileNotFoundException)
            {
                TempData["ErrorMessage"] = "File not found on server.";
                return RedirectToAction("ContentList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading content");
                TempData["ErrorMessage"] = "Error downloading file.";
                return RedirectToAction("ContentList");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        private bool CourseExists(string id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }
    }
}