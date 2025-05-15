using Microsoft.AspNetCore.Identity;

namespace ITSL_Administration.Models
{
    public class Users : IdentityUser
    {

        public string FullName { get; set; }

        public int? Age { get; set; }

        public string? IDNumber { get; set; }

        public string? City { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public bool? isVolunteer { get; set; }

        public double? AmountDonated { get; set; }

        public string? CampusName { get; set; }

    }
}
