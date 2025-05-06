using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Enrollment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string EnrollmentID { get; set; }//PK

        public string ParticipantID { get; set; }//FK 

        public string ModuleID { get; set; }

        public string TutorID { get; set; }

        public string LecturerID { get; set; }

        public double? Grade { get; set; }

        public bool IsPassed { get; set; }

        //Navigation Properties

        public Lecturer Lecturer { get; set; }

        public Tutor Tutor { get; set; }

        public Course Course { get; set; }

        public Participant Participant { get; set; }

        // In your Enrollment model class
        [NotMapped]
        public string CourseName => Course?.ModuleName ?? "N/A";

        [NotMapped]
        public string ParticipantName =>
            Participant != null ? $"{Participant.Name} {Participant.Surname}" : "N/A";

        [NotMapped]
        public string TutorName =>
            Tutor != null ? $"{Tutor.Name} {Tutor.Surname}" : "N/A";

        [NotMapped]
        public string LecturerName =>
            Lecturer != null ? $"{Lecturer.Name} {Lecturer.Surname}" : "N/A";
    }
}
