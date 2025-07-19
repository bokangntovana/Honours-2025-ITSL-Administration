using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public string ParticipantId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters.")]
        public string? Notes { get; set; }

        [Display(Name = "File Path")]
        public string? FilePath { get; set; }

        [Display(Name = "Original File Name")]
        public string? OriginalFileName { get; set; }

        [Range(0, 1000, ErrorMessage = "Grade must be between 0 and 1000")]
        public decimal? Grade { get; set; }

        [StringLength(500, ErrorMessage = "Feedback cannot be longer than 500 characters.")]
        public string? Feedback { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Graded Date")]
        public DateTime? GradedDate { get; set; }

        // Add these new properties
        [Display(Name = "Auto Graded Score")]
        public decimal? AutoGradedScore { get; set; }

        // Navigation properties
        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; }

        [ForeignKey("ParticipantId")]
        public Users Participant { get; set; }

        // Navigation property for quiz answers
        public ICollection<QuizAnswer> QuizAnswers { get; set; }
    }
}
