using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Participant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string ParticipantID { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        public int Age { get; set; }

        //public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string City { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        //Navigation Properties

        public ICollection<Enrollment> Enrollments { get; set; }



    }
}
