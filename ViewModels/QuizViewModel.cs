using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSL_Administration.ViewModels
{
    public class QuizViewModel
    {
        // Identifiers
        public string QuizId { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;

        // Quiz & Course Info
        public string CourseName { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }

        // Question Data
        public List<QuestionItem> Questions { get; set; } = new List<QuestionItem>();

        // NEW: Store all user answers across all pages
        public Dictionary<string, string> AllUserAnswers { get; set; } = new Dictionary<string, string>();

        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        public int TotalQuestions { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalQuestions / (double)PageSize);

        // Navigation properties for pagination
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int PreviousPage => CurrentPage - 1;
        public int NextPage => CurrentPage + 1;
    }

    public class QuestionItem
    {
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public List<SelectListItem> Options { get; set; } = new List<SelectListItem>();
        public string UserAnswer { get; set; } = string.Empty;
    }
}