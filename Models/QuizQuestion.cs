using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class QuizQuestion
    {
        public int QuizQuestionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Question text cannot be longer than 500 characters.")]
        public string QuestionText { get; set; }

        [Required]
        public QuestionType QuestionType { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        public int Points { get; set; } = 1;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; }
        public ICollection<QuestionOption> Options { get; set; }
        public ICollection<QuizAnswer> Answers { get; set; }
    }

}
