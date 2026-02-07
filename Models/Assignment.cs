using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Assignment
    {
        [Key]
        public string AssignmentID { get; set; } = Guid.NewGuid().ToString();
        public string? CourseId { get; set; }
        [Required(ErrorMessage = "Assignment Title is required.")]
        [Display(Name = "Assignment Title")]
        public string? Title { get; set; }
        [Required(ErrorMessage = "Assignment Description is required.")]
        [Display(Name = "Assignment Description")]
        public string? Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Due Date is required.")]
        [Display(Name = "Assignment Due Date")]
        public DateTime DueDate { get; set; }
        //[Required(ErrorMessage = "You need to set the total mark for the assignment")]
        [Range(0, double.MaxValue)]
        [Display(Name = "Total Mark")]
        public double SetAssignmentMark { get; set; }
        //[Required(ErrorMessage = "You need to set the weight for the assignment")]
        [Range(0, 1)]
        public double Weight { get; set; }

        [Required(ErrorMessage = "You need to determine the type for the assignment")]
        [Display(Name = "Assignment Type")]
        public AssignmentType AssignmentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //Navigation properties
        //The course this assignment belongs to
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
        //The Files for the Assignment instructions
        public ICollection<UploadedFile> Files { get; set; } = new List<UploadedFile>();
        //The list of submissions for the Assignment
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
