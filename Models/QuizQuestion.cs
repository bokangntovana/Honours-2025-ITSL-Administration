using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class QuizQuestion
    {

        [Key]
        public string QuestionID { get; set; } = Guid.NewGuid().ToString();

        public string QuizId { get; set; } = string.Empty;

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; }

        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
    }
}
