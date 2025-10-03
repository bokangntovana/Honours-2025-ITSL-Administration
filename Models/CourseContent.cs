using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.Models
{
    public class CourseContent
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public string? FileId { get; set; }
        public UploadedFile? File { get; set; }

        // Link to course
        public string? CourseId { get; set; }
        public Course? Course { get; set; }
    }

}
