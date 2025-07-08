namespace ITSL_Administration.Models
{
    public class CourseContent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string CourseID { get; set; }
        public Courses Course { get; set; }
    }
}
