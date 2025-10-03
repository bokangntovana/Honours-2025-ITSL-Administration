// Models/Donation.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    public class Donation
    {
        [Key]
        public int DonationId { get; set; }

        
        public string? DonorId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "zar";

        [Required]
        public DateTime DonationDate { get; set; } = DateTime.Now;

        [Required]
        public string PaymentStatus { get; set; }

        //Stripe MetaData
        public string? StripePaymentIntentId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? ReceiptUrl { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("DonorId")]
        public virtual User Donor { get; set; }
    }
}