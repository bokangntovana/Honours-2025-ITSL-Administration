using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Donor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string DonorID { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        //public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public int Age { get; set; }

        public double AmountDonated { get; set; }

        public bool IsActiveVolunteer { get; set; }

        //Navigation Properties
    }
}
