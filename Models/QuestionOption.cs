using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class QuestionOption
    {
        public int QuestionOptionId { get; set; }

        [Required]
        public int QuizQuestionId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Option text cannot be longer than 200 characters.")]
        public string OptionText { get; set; }

        [Required]
        public bool IsCorrect { get; set; } = false;

        // Navigation properties
        [ForeignKey("QuizQuestionId")]
        public QuizQuestion QuizQuestion { get; set; }
    }
}
