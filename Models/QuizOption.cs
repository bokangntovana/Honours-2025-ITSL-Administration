using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class QuizOption
    {
        [Key]
        public string OptionID { get; set; } = Guid.NewGuid().ToString();
        [Required(ErrorMessage = "Option text is required.")]
        public string? OptionText { get; set; }

        public bool IsCorrect { get; set; } = false;

        public string? QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public QuizQuestion? Question { get; set; }

    }
}
