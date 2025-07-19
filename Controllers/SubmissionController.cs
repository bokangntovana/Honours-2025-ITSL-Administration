using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class SubmissionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public SubmissionController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Submission/MySubmissions
        public async Task<IActionResult> MySubmissions()
        {
            var user = await _userManager.GetUserAsync(User);
            var submissions = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .Where(s => s.ParticipantId == user.Id)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();

            return View(submissions);
        }

        // GET: Submission/Create/{assignmentId}
        public async Task<IActionResult> Create(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            ViewBag.Assignment = assignment;
            return View();
        }

        // POST: Submission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int assignmentId, string notes, IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            var assignment = await _context.Assignments.FindAsync(assignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a file for your submission.");
                ViewBag.Assignment = assignment;
                return View();
            }

            try
            {
                // Create upload directory if it doesn't exist
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "submissions");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var fileName = $"{user.Id}_{assignmentId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save to database
                var submission = new Submission
                {
                    AssignmentId = assignmentId,
                    ParticipantId = user.Id,
                    Notes = notes,
                    FilePath = Path.Combine("submissions", fileName),
                    OriginalFileName = file.FileName,
                    SubmissionDate = DateTime.Now
                };

                _context.Add(submission);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Submission uploaded successfully!";
                return RedirectToAction(nameof(MySubmissions));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while uploading your submission. Please try again.");
                ViewBag.Assignment = assignment;
                return View();
            }
        }

        // GET: Submission/Download/{id}
        public async Task<IActionResult> Download(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            // Only allow download if user is the submitter, admin, or lecturer
            if (submission.ParticipantId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("Lecturer"))
            {
                return Forbid();
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", submission.FilePath);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(path), submission.OriginalFileName);
        }

        // GET: Submission/TakeQuiz/5
        public async Task<IActionResult> TakeQuiz(int assignmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.QuizQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || assignment.AssignmentType != AssignmentType.Quiz)
            {
                return NotFound();
            }

            // Check if already submitted
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.ParticipantId == user.Id);

            if (existingSubmission != null)
            {
                TempData["Message"] = "You have already submitted this quiz.";
                return RedirectToAction(nameof(MySubmissions));
            }

            return View(assignment);
        }

        // POST: Submission/SubmitQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(int assignmentId, Dictionary<int, string> answers, Dictionary<int, int?> selectedOptions)
        {
            var user = await _userManager.GetUserAsync(User);
            var assignment = await _context.Assignments
                .Include(a => a.QuizQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || assignment.AssignmentType != AssignmentType.Quiz)
            {
                return NotFound();
            }

            // Check if already submitted
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.ParticipantId == user.Id);

            if (existingSubmission != null)
            {
                TempData["Message"] = "You have already submitted this quiz.";
                return RedirectToAction(nameof(MySubmissions));
            }

            // Create new submission
            var submission = new Submission
            {
                AssignmentId = assignmentId,
                ParticipantId = user.Id,
                SubmissionDate = DateTime.Now
            };

            _context.Add(submission);
            await _context.SaveChangesAsync();

            decimal totalScore = 0;
            decimal maxPossibleScore = assignment.QuizQuestions.Sum(q => q.Points);

            // Process answers
            foreach (var question in assignment.QuizQuestions)
            {
                var answer = new QuizAnswer
                {
                    SubmissionId = submission.SubmissionId,
                    QuizQuestionId = question.QuizQuestionId
                };

                if (question.QuestionType == QuestionType.MultipleChoice && selectedOptions.ContainsKey(question.QuizQuestionId))
                {
                    answer.SelectedOptionId = selectedOptions[question.QuizQuestionId];

                    // Auto-grade multiple choice questions
                    if (answer.SelectedOptionId.HasValue)
                    {
                        var selectedOption = question.Options.FirstOrDefault(o => o.QuestionOptionId == answer.SelectedOptionId);
                        if (selectedOption != null && selectedOption.IsCorrect)
                        {
                            answer.PointsAwarded = question.Points;
                            totalScore += question.Points;
                        }
                    }
                }
                else if (answers.ContainsKey(question.QuizQuestionId))
                {
                    answer.AnswerText = answers[question.QuizQuestionId];
                    // Short answer and true/false questions need manual grading
                }

                _context.Add(answer);
            }

            // Calculate auto-graded score percentage
            submission.AutoGradedScore = maxPossibleScore > 0 ? (totalScore / maxPossibleScore) * 100 : 0;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Quiz submitted successfully!";
            return RedirectToAction(nameof(MySubmissions));
        }

        // GET: Submission/QuizDetails/5
        public async Task<IActionResult> QuizDetails(int submissionId)
        {
            var user = await _userManager.GetUserAsync(User);
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.QuizQuestions)
                        .ThenInclude(q => q.Options)
                .Include(s => s.QuizAnswers)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                return NotFound();
            }

            // Only allow submitter, admin, or lecturer to view
            if (submission.ParticipantId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("Lecturer"))
            {
                return Forbid();
            }

            return View(submission);
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
    }
}

