using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Courses
    {
        [Key]
        [Display(Name = "Course ID")]
        public string CourseID { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Course code is Required")]
        [Display(Name = "Course Code")]
        [StringLength(20, ErrorMessage = "Course code cannot be longer than 20 characters.")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Name is Required")]
        [StringLength(100, ErrorMessage = "Course Name cannot be longer than 100 characters.")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Credits is Required")]
        [Display(Name = "Course Credits")]
        public int CourseCredits { get; set; }

        [StringLength(200, ErrorMessage = "Course Description cannot be longer than 200 characters.")]
        [Display(Name = "Course Description")]
        public string? CourseDescription { get; set; }

        //Navigation properties
        public ICollection<CourseContent> Contents { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}