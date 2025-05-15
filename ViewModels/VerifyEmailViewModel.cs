using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage ="Email is Required")]
        [EmailAddress]
        public string Email { get; set; }

    }
}
