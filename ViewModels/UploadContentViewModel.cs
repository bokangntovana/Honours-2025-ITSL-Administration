namespace ITSL_Administration.ViewModels
{
    public class UploadContentViewModel
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public IFormFile File { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
