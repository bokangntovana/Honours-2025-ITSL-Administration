using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        // custom properties also added 
        public int? Age { get; set; }
        public string? IDNumber { get; set; }
        public string? City { get; set; }
        public string? CampusName { get; set; }
    }
}