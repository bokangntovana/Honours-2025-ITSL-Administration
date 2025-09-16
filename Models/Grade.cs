using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Grade
    {
        [Key]
        public string GradeID { get; set; } = Guid.NewGuid().ToString();

        public  string? SubmissionId { get; set; }

        public string? AssignmentId { get; set; } 

        [Display(Name = "Date Recorded")]
        [Required]
        public DateTime DateRecorded { get; set; } = DateTime.Now;
        [Required]
        [Display(Name = "Marked By")]
        public string? MarkedBy { get; set; }
        [Required]
        [StringLength(500, ErrorMessage = "Feedback cannot exceed 500 characters")]
        //Consider AssignmentType attribute for grades using the Assignment nav property
        [Display(Name = "Assignment Feedback")]
        [DataType(DataType.MultilineText)]
        public string? GradesFeedback { get; set; }
        [Range(0, 100)]
        [Display(Name = "Obtained Mark for Assignment")]
        public double AssignmentMark { get; set; } = 0.00;
        [Range(0, 100)]
        [Required]
        [Display(Name = "Final Mark for Course")]
        public  double FinalMark { get; set; } = 0.00;
        [Required]
        [Display(Name = "Grade")]
        public  bool HasPassed { get; set; } = false;

        // Navigation properties
        [ForeignKey("SubmissionId")]
        public Submission? Submission { get; set; }

        [ForeignKey("AssignmentId")]
        public Assignment? Assignment { get; set; }


    }
}
