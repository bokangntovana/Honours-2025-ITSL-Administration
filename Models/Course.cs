using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.Models
{
    public class Course
    {
        [Key]
        public string CourseID { get; set; } = Guid.NewGuid().ToString();

       //[Required(ErrorMessage = "Course code is Required")]
        [Display(Name = "Course Code")]
        [StringLength(10, ErrorMessage = "Course code cannot be longer than 10 characters.")]
        public string? CourseCode { get; set; }

       //[Required(ErrorMessage = "Course Name is Required")]
        [Display(Name = "Course Name")]
        public string? CourseName { get; set; }

      // [Required(ErrorMessage = "Course Credits is Required")]
        [Display(Name = "Course Credits")]
        public int CourseCredits { get; set; }

        [StringLength(200, ErrorMessage = "Course Description cannot be longer than 200 characters.")]
        [Display(Name = "Course Description")]
        public string? CourseDescription { get; set; }

        // Navigation Properties
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<CourseContent> Contents { get; set; } = new List<CourseContent>();

    }
}
