using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Course
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string ModuleID { get; set; }

        public string ModuleName { get; set; }

        public int ModuleCredits { get; set; }

        public string? ModuleDescription { get; set; }

        //Navigation Properties

        public ICollection<Enrollment> Enrollments { get; set; }

    }
}
