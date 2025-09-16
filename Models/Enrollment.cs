using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Enrollment
    {
        public string? UserId { get; set; }
        public string? CourseId { get; set; }
        public CourseRole Role { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
    }

 
}
