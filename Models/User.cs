using Microsoft.AspNetCore.Identity;

namespace ITSL_Administration.Models
{
    public class User : IdentityUser
    {

        public string FullName { get; set; } =string.Empty;

        public int Age { get; set; } = 0;

        public string IDNumber { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public bool isVolunteer { get; set; } =false;

        public double AmountDonated { get; set; } = 0.00;

        public string CampusName { get; set; } = string.Empty;

    }
}
