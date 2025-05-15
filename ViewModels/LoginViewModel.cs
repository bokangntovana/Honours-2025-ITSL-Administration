using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Email is required to sign in")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required to sign in")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")] 
        public bool RememberMe { get; set; }
    }
}
