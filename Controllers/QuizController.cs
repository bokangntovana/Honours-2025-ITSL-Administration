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
            IQueryable<Quiz> query = _context.Quizzes
                .Include(q => q.Assignment)
                .ThenInclude(a => a.Course);

            // REMOVED: Enrollment filtering for non-admin/non-lecturer users
            // Now all authenticated users can see all quizzes
            // if (!(User.IsInRole("Admin") || User.IsInRole("Lecturer")))
            // {
            //     query = query.Where(q => q.Assignment.Course.Enrollment
            //         .Any(e => e.UserId == userId));
            // }

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

            if (!IsAdminOrLecturer()) return Forbid(); // REMOVED: courseId parameter

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

                // authorization check - REMOVED: courseId parameter
                if (!IsAdminOrLecturer())
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
            if (!IsAdminOrLecturer()) return Forbid(); // REMOVED: courseId parameter

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

        // Update the TakeQuiz method to handle POST navigation
        [HttpGet]
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
                PageSize = 3
            };

            // Get questions for current page
            var currentPageQuestions = quiz.Questions?
                .OrderBy(q => q.QuestionText)
                .Skip((page - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .ToList() ?? new List<QuizQuestion>();

            vm.Questions = currentPageQuestions.Select(q => new QuestionItem
            {
                QuestionId = q.QuestionID,
                QuestionText = q.QuestionText,
                Options = q.Options?.Select(o => new SelectListItem
                {
                    Value = o.OptionID,
                    Text = o.OptionText ?? ""
                }).ToList() ?? new List<SelectListItem>(),
                UserAnswer = string.Empty // Will be populated from sessionStorage by JavaScript
            }).ToList();

            return View(vm);
        }

        // Add this new method to handle navigation via POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NavigateQuiz(QuizViewModel model, string action)
        {
            // Get the quiz to calculate proper pagination
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizID == model.QuizId);

            if (quiz == null) return NotFound();

            int totalQuestions = quiz.Questions?.Count ?? 0;
            int pageSize = 3; // Should match your view model
            int totalPages = (int)Math.Ceiling(totalQuestions / (double)pageSize);

            // Calculate target page based on action
            int targetPage = model.CurrentPage;

            if (action == "next" && model.CurrentPage < totalPages)
            {
                targetPage = model.CurrentPage + 1;
            }
            else if (action == "previous" && model.CurrentPage > 1)
            {
                targetPage = model.CurrentPage - 1;
            }

            // Always redirect to the TakeQuiz GET method
            return RedirectToAction("TakeQuiz", new
            {
                quizId = model.QuizId,
                page = targetPage
            });
        }

        //SubmitQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(QuizViewModel model, string action)
        {
            if (action != "submit")
            {
                // If not a submit action, redirect back (safety fallback)
                return RedirectToAction("TakeQuiz", new { quizId = model.QuizId, page = model.CurrentPage });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check for existing submission (block retakes)
            var existingSubmission = await _context.Submissions
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.ParticipantId == userId && s.AssignmentId == model.AssignmentId);

            if (existingSubmission != null)
            {
                return RedirectToAction("QuizResult", new { submissionId = existingSubmission.SubmissionID });
            }

            // Create new submission
            var submission = new Submission
            {
                SubmissionID = Guid.NewGuid().ToString(),
                ParticipantId = userId,
                AssignmentId = model.AssignmentId,
                DateSubmitted = DateTime.Now
            };

            // Calculate final score
            var allQuestions = await _context.QuizQuestions
                .Where(q => q.QuizId == model.QuizId)
                .Include(q => q.Options)
                .ToListAsync();

            double correctAnswers = 0.0;
            int totalQuestions = allQuestions.Count;

            // Calculate score based on submitted answers
            // Note: The model.Questions here contains ALL questions from the final submission form
            // (populated by JavaScript in the view)
            foreach (var question in allQuestions)
            {
                // Find the user's answer for this question
                var userAnswer = model.Questions?
                    .FirstOrDefault(q => q.QuestionId == question.QuestionID)?.UserAnswer;

                if (!string.IsNullOrEmpty(userAnswer))
                {
                    var selectedOption = question.Options?
                        .FirstOrDefault(o => o.OptionID == userAnswer);

                    if (selectedOption?.IsCorrect == true)
                    {
                        correctAnswers++;
                    }
                }
            }

            double percentage = totalQuestions > 0 ? (correctAnswers / totalQuestions) * 100.0 : 0.0;
            bool hasPassed = percentage >= 50.0;

            // Create grade
            var grade = new Grade
            {
                GradeID = Guid.NewGuid().ToString(),
                SubmissionId = submission.SubmissionID,
                AssignmentId = model.AssignmentId,
                AssignmentMark = percentage,
                FinalMark = percentage,
                HasPassed = hasPassed,
                MarkedBy = "System",
                DateRecorded = DateTime.Now,
                GradesFeedback = hasPassed
                    ? "Well done, you passed!"
                    : "Unfortunately, you did not pass. Please review and try again."
            };

            submission.Grade = grade;

            _context.Submissions.Add(submission);
            _context.Grades.Add(grade);

            await _context.SaveChangesAsync();

            return RedirectToAction("QuizResult", new { submissionId = submission.SubmissionID });
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
                    FinalMark = 0.0, // REMOVED: Weight calculation
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
        // HELPERS - UPDATED TO REMOVE ENROLLMENT
        // ================================
        private bool IsAdminOrLecturer()
        {
            // REMOVED: Enrollment-based check
            // Simplified to pure role-based check
            return User.IsInRole("Admin") || User.IsInRole("Lecturer");
        }
    }
}