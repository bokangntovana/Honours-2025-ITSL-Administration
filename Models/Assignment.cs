using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Assignment
    {
        public int AssignmentId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

        [Required]
        [Display(Name = "Maximum Points")]
        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        public int MaxPoints { get; set; } = 100;

        [Required]
        public string CourseId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Assignment Type")]
        public AssignmentType AssignmentType { get; set; } = AssignmentType.RegularAssignment;

        // Navigation properties
        [ForeignKey("CourseId")]
        public Courses Course { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<QuizQuestion> QuizQuestions { get; set; }

    }
}
