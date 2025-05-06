using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Lecturer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string LecturerID { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        //public string Password { get; set; }

        public string PhoneNumber { get; set; }

        //Navigation Properties

        public ICollection<Enrollment> Enrollments { get; set; }
    }

}
