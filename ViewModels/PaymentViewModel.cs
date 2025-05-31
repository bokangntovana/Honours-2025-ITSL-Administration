
using System.ComponentModel.DataAnnotations;

namespace ITSL_Administration.ViewModels
{
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "Donation amount is required")]
        [Range(1, 100000, ErrorMessage = "Amount must be at least R1")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "zar";

        [Required(ErrorMessage = "Email is required for payment confirmation")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        // Stripe Token from client-side
        public string StripeToken { get; set; }
    }
}
