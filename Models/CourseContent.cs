using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.Models
{
    public class CourseContent
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;


        [Required]
        public string? FileId { get; set; }
        public UploadedFile? File { get; set; }

        // Link to course
        [Required]
        public string? CourseId { get; set; }
        public Course? Course { get; set; }
    }

}
