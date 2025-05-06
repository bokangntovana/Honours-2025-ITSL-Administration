using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Tutor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string TutorID { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        //public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string? Campus { get; set; }

        public bool IsRegisteredForITSL { get; set; }

        //Navigation Properties

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
