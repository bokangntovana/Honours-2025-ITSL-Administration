using ITSL_Administration.Models;

namespace ITSL_Administration.ViewModels
{
    public class EmailViewModel
    {
        public List<string>? SelectedEmails { get; set; } = new();
        public bool SendToAll { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public List<IFormFile>? Attachments { get; set; } // 

        // NEW: To display users in the dropdown
        public List<User>? Users { get; set; }
    }
}
