using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.ViewModels
{
    public class DonorRegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is Required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at least {2} and at maximum {1} characters long")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "The password and confirmation password do not match")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
        public int? Age { get; set; }

        [Display(Name = "ID Number")]
        [StringLength(13, ErrorMessage = "ID Number cannot be longer than 13 characters")]
        public string? IDNumber { get; set; }

        [StringLength(50, ErrorMessage = "City name cannot be longer than 50 characters")]
        public string? City { get; set; }

        //[Display(Name = "Campus Name")]
        //[StringLength(100, ErrorMessage = "Campus name cannot be longer than 100 characters")]
        //public string? CampusName { get; set; }
    }
}
