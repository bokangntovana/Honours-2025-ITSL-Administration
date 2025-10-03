using ITSL_Administration.Models;

namespace ITSL_Administration.ViewModels
{
    public class GradeCreateViewModel
    {
        public Grade Grade { get; set; } = new Grade();
        public Submission Submission { get; set; } = new Submission();
    }
}
