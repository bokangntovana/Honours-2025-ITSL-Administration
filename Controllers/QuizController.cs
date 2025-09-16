using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITSL_Administration.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly AppDbContext _context;

        public QuizController(AppDbContext context)
        {
            _context = context;
        }

        // ================================
        // QUIZ CREATION & MANAGEMENT
        // ================================

        public IActionResult Create(string courseId)
        {
            var quiz = new Quiz
            {
                Assignment = new Assignment
                {
                    CourseId = courseId,
                    AssignmentType = AssignmentType.Quiz
                }
            };
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuizID,Assignment")] Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                quiz.QuizID = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(quiz.Assignment.AssignmentID))
                    quiz.Assignment.AssignmentID = Guid.NewGuid().ToString();

                quiz.Assignment.AssignmentType = AssignmentType.Quiz;
                quiz.Assignment.CreatedAt = DateTime.Now;

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                TempData["AlertMessage"] = "Quiz created successfully.";
                TempData["AlertType"] = "success";

                return RedirectToAction(nameof(AvailableQuiz));
            }
            return View(quiz);
        }

        public async Task<IActionResult> AvailableQuiz()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Quiz> query = _context.Quizzes
                .Include(q => q.Assignment)
                .ThenInclude(a => a.Course);

            if (!(User.IsInRole("Admin") || User.IsInRole("Lecturer")))
            {
                query = query.Where(q => q.Assignment.Course.Enrollments
                    .Any(e => e.UserId == userId));
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> EditQuiz(string id)
        {
            if (id == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Assignment)
                .Include(q => q.Questions)
                .ThenInclude(qn => qn.Options)
                .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null) return NotFound();

            if (!IsAdminOrLecturer(quiz.Assignment.CourseId)) return Forbid();

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuiz(string id, [Bind("QuizID,Assignment")] Quiz quiz)
        {
            if (quiz == null) return BadRequest();
            if (string.IsNullOrEmpty(id)) id = quiz.QuizID;

            if (id != quiz.QuizID) return NotFound();

            if (ModelState.IsValid)
            {
                var existingQuiz = await _context.Quizzes
                    .Include(q => q.Assignment)
                    .FirstOrDefaultAsync(q => q.QuizID == id);

                if (existingQuiz == null) return NotFound();

                // authorization check
                if (!IsAdminOrLecturer(existingQuiz.Assignment.CourseId))
                    return Forbid();

                // Update only the fields you want to allow editing
                existingQuiz.Assignment.Title = quiz.Assignment.Title;
                existingQuiz.Assignment.Description = quiz.Assignment.Description;
                existingQuiz.Assignment.DueDate = quiz.Assignment.DueDate;
                existingQuiz.Assignment.SetAssignmentMark = quiz.Assignment.SetAssignmentMark;

                try
                {
                    await _context.SaveChangesAsync();

                    TempData["AlertMessage"] = "Quiz updated successfully.";
                    TempData["AlertType"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Quizzes.Any(e => e.QuizID == quiz.QuizID))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(AvailableQuiz));
            }

            return View(quiz);
        }

        public async Task<IActionResult> DeleteQuiz(string id)
        {
            if (id == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Assignment)
                .FirstOrDefaultAsync(q => q.QuizID == id);

            if (quiz == null) return NotFound();
            if (!IsAdminOrLecturer(quiz.Assignment.CourseId)) return Forbid();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            TempData["AlertMessage"] = "Quiz deleted.";
            TempData["AlertType"] = "warning";

            return RedirectToAction(nameof(AvailableQuiz));
        }

        // ================================
        // QUESTIONS & OPTIONS
        // ================================

        public IActionResult AddQuestion(string quizId)
        {
            ViewBag.QuizId = quizId;
            return View(new QuizQuestion { QuizId = quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion([Bind("QuestionID,QuestionText,QuizId")] QuizQuestion question)
        {
            if (ModelState.IsValid)
            {
                question.QuestionID = Guid.NewGuid().ToString();
                _context.QuizQuestions.Add(question);
                await _context.SaveChangesAsync();

                TempData["AlertMessage"] = "Question created successfully.";
                TempData["AlertType"] = "success";

                return RedirectToAction("EditQuiz", new { id = question.QuizId });
            }
            return View(question);
        }

        public IActionResult AddOption(string questionId)
        {
            return View(new QuizOption { QuestionId = questionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOption([Bind("OptionID,OptionText,IsCorrect,QuestionId")] QuizOption option)
        {
            if (ModelState.IsValid)
            {
                option.OptionID = Guid.NewGuid().ToString();

                // Ensure only one correct option per question
                if (option.IsCorrect)
                {
                    var existingOptions = _context.QuizOptions
                        .Where(o => o.QuestionId == option.QuestionId && o.IsCorrect);
                    foreach (var o in existingOptions)
                        o.IsCorrect = false;
                }

                _context.QuizOptions.Add(option);
                await _context.SaveChangesAsync();

                TempData["AlertMessage"] = "Option created successfully.";
                TempData["AlertType"] = "success";

                // Load the question to find the parent quiz id for redirect
                var question = await _context.QuizQuestions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuestionID == option.QuestionId);

                return RedirectToAction("EditQuiz", new { id = question?.QuizId });
            }
            return View(option);
        }

        [HttpGet]
        public async Task<IActionResult> EditOption(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var option = await _context.QuizOptions
                .FirstOrDefaultAsync(o => o.OptionID == id);

            if (option == null) return NotFound();

            return View(option);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOption([Bind("OptionID,OptionText,IsCorrect,QuestionId")] QuizOption option)
        {
            if (!ModelState.IsValid) return View(option);

            try
            {
                // If setting this option to correct, clear others for the question
                if (option.IsCorrect)
                {
                    var existingOptions = _context.QuizOptions.Where(o => o.QuestionId == option.QuestionId && o.OptionID != option.OptionID);
                    foreach (var o in existingOptions)
                        o.IsCorrect = false;
                }

                _context.Update(option);
                await _context.SaveChangesAsync();

                TempData["AlertMessage"] = "Option updated successfully.";
                TempData["AlertType"] = "success";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.QuizOptions.Any(o => o.OptionID == option.OptionID))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("EditQuestion", new { id = option.QuestionId });
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(string id)
        {
            if (id == null) return NotFound();

            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionID == id);

            if (question == null) return NotFound();

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion([Bind("QuestionID,QuestionText,QuizId")] QuizQuestion question)
        {
            if (!ModelState.IsValid) return View(question);

            try
            {
                _context.Update(question);
                await _context.SaveChangesAsync();

                TempData["AlertMessage"] = "Question updated successfully.";
                TempData["AlertType"] = "success";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.QuizQuestions.Any(q => q.QuestionID == question.QuestionID))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("EditQuiz", new { id = question.QuizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var option = await _context.QuizOptions
                .Include(o => o.Question)
                .FirstOrDefaultAsync(o => o.OptionID == id);

            if (option == null) return NotFound();

            var quizId = option.Question?.QuizId;

            _context.QuizOptions.Remove(option);
            await _context.SaveChangesAsync();

            TempData["AlertMessage"] = "Option deleted.";
            TempData["AlertType"] = "warning";

            return RedirectToAction("EditQuiz", new { id = quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var question = await _context.QuizQuestions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuestionID == id);

            if (question == null) return NotFound();

            var quizId = question.QuizId;

            // Remove question (options cascade configured in OnModelCreating)
            _context.QuizQuestions.Remove(new QuizQuestion { QuestionID = id });
            await _context.SaveChangesAsync();

            TempData["AlertMessage"] = "Question deleted.";
            TempData["AlertType"] = "warning";

            return RedirectToAction("EditQuiz", new { id = quizId });
        }

        // ================================
        // TAKING & SUBMITTING QUIZZES
        // ================================

        public async Task<IActionResult> TakeQuiz(string quizId, int page = 1)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Assignment)
                .ThenInclude(a => a.Course)
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizID == quizId);

            if (quiz == null) return NotFound();

            var vm = new QuizViewModel
            {
                QuizId = quiz.QuizID,
                AssignmentId = quiz.AssignmentId,
                QuizTitle = quiz.Assignment?.Title ?? "Quiz",
                CourseName = quiz.Assignment?.Course?.CourseName ?? "Unknown Course",
                DueDate = quiz.Assignment?.DueDate ?? DateTime.MinValue,
                CurrentPage = page,
                TotalQuestions = quiz.Questions?.Count ?? 0,
                PageSize = 3 // Set your desired page size
            };

            vm.Questions = quiz.Questions?
                .OrderBy(q => q.QuestionText)
                .Skip((page - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .Select(q => new QuestionItem
                {
                    QuestionId = q.QuestionID,
                    QuestionText = q.QuestionText,
                    Options = q.Options?.Select(o => new SelectListItem
                    {
                        Value = o.OptionID,
                        Text = o.OptionText ?? ""
                    }).ToList() ?? new List<SelectListItem>()
                }).ToList() ?? new List<QuestionItem>();

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(QuizViewModel model, string action)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get submission if it already exists
            var submission = await _context.Submissions
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.ParticipantId == userId && s.AssignmentId == model.AssignmentId);

            //  Block retakes when submitting
            if (submission != null && action == "submit")
            {
                return RedirectToAction("QuizResult", new { submissionId = submission.SubmissionID });
            }

            // Create new submission if none exists
            if (submission == null)
            {
                submission = new Submission
                {
                    SubmissionID = Guid.NewGuid().ToString(),
                    ParticipantId = userId,
                    AssignmentId = model.AssignmentId,
                    DateSubmitted = DateTime.Now
                };

                // Create Grade immediately
                submission.Grade = new Grade
                {
                    GradeID = Guid.NewGuid().ToString(),
                    SubmissionId = submission.SubmissionID,
                    AssignmentId = model.AssignmentId,
                    AssignmentMark = 0.0,
                    FinalMark = 0.0, // unused
                    HasPassed = false,
                    MarkedBy = "System",
                    DateRecorded = DateTime.Now,
                    GradesFeedback = string.Empty
                };

                _context.Submissions.Add(submission);
                _context.Grades.Add(submission.Grade);
            }
            else if (submission.Grade == null)
            {
                // Safety: ensure grade exists
                submission.Grade = new Grade
                {
                    GradeID = Guid.NewGuid().ToString(),
                    SubmissionId = submission.SubmissionID,
                    AssignmentId = model.AssignmentId,
                    AssignmentMark = 0.0,
                    FinalMark = 0.0,
                    HasPassed = false,
                    MarkedBy = "System",
                    DateRecorded = DateTime.Now,
                    GradesFeedback = string.Empty
                };

                _context.Grades.Add(submission.Grade);
            }

            //  Calculate score
            double correctAnswers = 0.0;
            foreach (var q in model.Questions ?? Enumerable.Empty<QuestionItem>())
            {
                if (!string.IsNullOrEmpty(q.UserAnswer))
                {
                    var option = await _context.QuizOptions
                        .FirstOrDefaultAsync(o => o.OptionID == q.UserAnswer);

                    if (option?.IsCorrect == true)
                    {
                        correctAnswers++;
                    }
                }
            }

            if (action == "submit")
            {
                // Count total questions in this quiz
                var totalQuestions = await _context.QuizQuestions
                    .CountAsync(q => q.QuizId == model.QuizId);

                if (totalQuestions > 0)
                {
                    submission.Grade.AssignmentMark = (correctAnswers / totalQuestions) * 100.0;
                }
                else
                {
                    submission.Grade.AssignmentMark = 0.0;
                }

                // FinalMark left unused
                submission.Grade.FinalMark = 0.0;

                // Pass/fail logic
                submission.Grade.HasPassed = submission.Grade.AssignmentMark >= 50.0;
                submission.Grade.GradesFeedback = submission.Grade.HasPassed
                    ? "Well done, you passed!"
                    : "Unfortunately, you did not pass. Please review and try again.";

                submission.Grade.MarkedBy ??= "System";
                submission.Grade.DateRecorded = DateTime.Now;
                submission.DateSubmitted = DateTime.Now;

                await _context.SaveChangesAsync();

                return RedirectToAction("QuizResult", new { submissionId = submission.SubmissionID });
            }

            // Navigate (next/prev page)
            return RedirectToAction("TakeQuiz", new { quizId = model.QuizId, page = model.CurrentPage });
        }



        public async Task<IActionResult> QuizResult(string submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .ThenInclude(a => a.Course)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.SubmissionID == submissionId);

            if (submission == null) return NotFound();

            // If a Grade wasn't created previously, create and persist a default Grade so the view can safely render
            if (submission.Grade == null)
            {
                var newGrade = new Grade
                {
                    GradeID = Guid.NewGuid().ToString(),
                    SubmissionId = submission.SubmissionID,
                    AssignmentId = submission.AssignmentId,
                    AssignmentMark = 0.0,
                    FinalMark = 0.0,
                    HasPassed = false,
                    MarkedBy = "System",
                    DateRecorded = DateTime.Now,
                    GradesFeedback = string.Empty
                };

                // Attach and save
                _context.Grades.Add(newGrade);
                await _context.SaveChangesAsync();

                // Re-query to include the newly created Grade
                submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.SubmissionID == submissionId);

                if (submission == null) return NotFound();
            }

            return View(submission);
        }

        // ================================
        // HELPERS
        // ================================
        private bool IsAdminOrLecturer(string courseId)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Lecturer"))
                return true;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.Enrollments.Any(e =>
                e.UserId == userId &&
                e.CourseId == courseId &&
                (e.Role == CourseRole.Lecturer || e.Role == CourseRole.Admin));
        }
    }




}