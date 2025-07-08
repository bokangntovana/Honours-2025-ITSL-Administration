using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public string ParticipantId { get; set; }

        [Required]
        public string CourseId { get; set; }

        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        // Possible status values: "Pending", "Approved", "Rejected", "Completed"
        public string Status { get; set; } = "Pending";

        // Grading properties
        [Range(0, 100, ErrorMessage = "Grade must be between 0 and 100")]
        [Display(Name = "Grade")]
        public decimal? Grade { get; set; }

        [Display(Name = "Grade Symbol")]
        public string? GradeSymbol { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Grade Date")]
        public DateTime? GradeDate { get; set; }

        [StringLength(500, ErrorMessage = "Feedback cannot be longer than 500 characters.")]
        public string? GradeFeedback { get; set; }

        // Method to calculate grade symbol
        public void CalculateGradeSymbol()
        {
            if (!Grade.HasValue) return;

            GradeSymbol = Grade.Value switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }



        // Navigation properties
        [ForeignKey("ParticipantId")]
        public Users Participant { get; set; }

        [ForeignKey("CourseId")]
        public Courses Course { get; set; }

    }
}
