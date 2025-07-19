using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSL_Administration.Models;
using ITSL_Administration.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ITSL_Administration.Controllers
{
    [Authorize(Roles = "Admin,Lecturer")]
    public class QuizController : Controller
    {
        private readonly AppDbContext _context;

        public QuizController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Quiz/ManageQuestions/5
        public async Task<IActionResult> ManageQuestions(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.QuizQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || assignment.AssignmentType != AssignmentType.Quiz)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // GET: Quiz/CreateQuestion/5
        public async Task<IActionResult> CreateQuestion(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || assignment.AssignmentType != AssignmentType.Quiz)
            {
                return NotFound();
            }

            ViewBag.Assignment = assignment;
            return View(new QuizQuestion { AssignmentId = assignmentId });
        }

        // POST: Quiz/CreateQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion([Bind("AssignmentId,QuestionText,QuestionType,Points,DisplayOrder")] QuizQuestion question)
        {
            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Question added successfully!";

                // For multiple choice questions, redirect to add options
                if (question.QuestionType == QuestionType.MultipleChoice)
                {
                    return RedirectToAction(nameof(ManageOptions), new { questionId = question.QuizQuestionId });
                }

                return RedirectToAction(nameof(ManageQuestions), new { assignmentId = question.AssignmentId });
            }

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == question.AssignmentId);

            ViewBag.Assignment = assignment;
            return View(question);
        }

        // GET: Quiz/EditQuestion/5
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(q => q.QuizQuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Quiz/EditQuestion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(int id, [Bind("QuizQuestionId,AssignmentId,QuestionText,QuestionType,Points,DisplayOrder")] QuizQuestion question)
        {
            if (id != question.QuizQuestionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Question updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.QuizQuestionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ManageQuestions), new { assignmentId = question.AssignmentId });
            }

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == question.AssignmentId);

            ViewBag.Assignment = assignment;
            return View(question);
        }

        // GET: Quiz/DeleteQuestion/5
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(q => q.QuizQuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Quiz/DeleteQuestion/5
        [HttpPost, ActionName("DeleteQuestion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestionConfirmed(int id)
        {
            var question = await _context.QuizQuestions.FindAsync(id);
            var assignmentId = question.AssignmentId;
            _context.QuizQuestions.Remove(question);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Question deleted successfully!";
            return RedirectToAction(nameof(ManageQuestions), new { assignmentId });
        }

        // GET: Quiz/ManageOptions/5
        public async Task<IActionResult> ManageOptions(int questionId)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Assignment)
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizQuestionId == questionId);

            if (question == null || question.QuestionType != QuestionType.MultipleChoice)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Quiz/AddOption
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOption(int questionId, string optionText, bool isCorrect)
        {
            var option = new QuestionOption
            {
                QuizQuestionId = questionId,
                OptionText = optionText,
                IsCorrect = isCorrect
            };

            _context.Add(option);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Option added successfully!";
            return RedirectToAction(nameof(ManageOptions), new { questionId });
        }

        // POST: Quiz/DeleteOption/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var option = await _context.QuestionOptions.FindAsync(id);
            var questionId = option.QuizQuestionId;
            _context.QuestionOptions.Remove(option);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Option deleted successfully!";
            return RedirectToAction(nameof(ManageOptions), new { questionId });
        }

        // GET: Quiz/ViewResults/5
        public async Task<IActionResult> ViewResults(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Participant)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.QuizAnswers)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || assignment.AssignmentType != AssignmentType.Quiz)
            {
                return NotFound();
            }

            return View(assignment);
        }

        private bool QuestionExists(int id)
        {
            return _context.QuizQuestions.Any(e => e.QuizQuestionId == id);
        }
    }

}
