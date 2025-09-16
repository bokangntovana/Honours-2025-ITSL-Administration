using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Quiz
    {
        [Key]
        public string QuizID { get; set; } = Guid.NewGuid().ToString();

        //[Required]
        public string AssignmentId { get; set; } = string.Empty;  // Link to Assignment

        [ForeignKey("AssignmentId")]
        public Assignment? Assignment { get; set; }  

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
