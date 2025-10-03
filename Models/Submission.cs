using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Submission
    {
        [Key]
        public string SubmissionID { get; set; } = Guid.NewGuid().ToString();

        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        public string ParticipantId { get; set; } = string.Empty;

        public string AssignmentId { get; set; } = string.Empty;

        //Navigation properties
        // The assignment this submission is for
        [ForeignKey("AssignmentId")]
        public Assignment? Assignment { get; set; }
        // Add this navigation property
        [InverseProperty("Submission")]
        public Grade? Grade { get; set; }
        [ForeignKey("ParticipantId")]
        public User? Participant { get; set; }
        // The files submitted for the assignment
        public ICollection<UploadedFile> Files { get; set; } = new List<UploadedFile>();
    }
}
