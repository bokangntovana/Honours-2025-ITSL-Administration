using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class QuizAnswer
    {
        public int QuizAnswerId { get; set; }

        [Required]
        public int SubmissionId { get; set; }

        [Required]
        public int QuizQuestionId { get; set; }

        [StringLength(500, ErrorMessage = "Answer cannot be longer than 500 characters.")]
        public string? AnswerText { get; set; }

        public int? SelectedOptionId { get; set; }

        [Display(Name = "Points Awarded")]
        public decimal? PointsAwarded { get; set; }

        // Navigation properties
        [ForeignKey("SubmissionId")]
        public Submission Submission { get; set; }

        [ForeignKey("QuizQuestionId")]
        public QuizQuestion QuizQuestion { get; set; }

        [ForeignKey("SelectedOptionId")]
        public QuestionOption? SelectedOption { get; set; }
    }
}
