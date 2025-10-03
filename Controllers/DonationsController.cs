using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace ITSL_Administration.Controllers
{
    [AllowAnonymous]
    public class DonationsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DonationsController> _logger;
        private readonly AppDbContext _context;


        public DonationsController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<DonationsController> logger,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        [HttpGet]
        public IActionResult Donations()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegisterDonor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDonor(DonorRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                FullName = model.Name,
                UserName = model.Email,
                NormalizedUserName = model.Email.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
              //  Age = model.Age,
                IDNumber = model.IDNumber,
                City = model.City,
                CampusName = model.CampusName,
                isVolunteer = false // Default to false for donors
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Donor role
                const string donorRole = "Donor";
                if (!await _roleManager.RoleExistsAsync(donorRole))
                {
                    ModelState.AddModelError("Role", "Invalid registration role");
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, donorRole);

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redirect to payment page
                return RedirectToAction("Payment", "Donations");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        //[Authorize(Roles = "Donor")]
        [HttpGet]
        public async Task<IActionResult> Payment()
        {
            // Get current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Initialize view model with user details
            var model = new PaymentViewModel
            {
                Name = user.FullName,
                Email = user.Email,
                Amount = 100.00m // Default donation amount
            };

            // Pass Stripe publishable key to view
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];

            return View(model);
        }

        //Payment Processing
       // [Authorize(Roles = "Donor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
                return View("Payment", model);
            }

            var user = await _userManager.GetUserAsync(User);

            // Create donation record with "pending" status
            var donation = new Donation
            {
                DonorId = user.Id,
                Amount = model.Amount,
                Currency = model.Currency,
                PaymentStatus = "pending",
                DonationDate = DateTime.Now
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(model.Amount * 100), // Convert to cents
                    Currency = model.Currency,
                    Description = $"Donation from {model.Name}",
                    ReceiptEmail = model.Email,
                    Metadata = new Dictionary<string, string>
                    {
                        { "DonationId", donation.DonationId.ToString() },
                        { "DonorId", user.Id }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                // Update donation with payment intent ID
                donation.StripePaymentIntentId = paymentIntent.Id;
                await _context.SaveChangesAsync();

                var confirmOptions = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = model.StripeToken,
                    ReturnUrl = Url.Action("PaymentConfirmation", "Donations", new { donationId = donation.DonationId }, Request.Scheme)
                };

                var confirmedIntent = await service.ConfirmAsync(paymentIntent.Id, confirmOptions);

                if (confirmedIntent.Status == "requires_action")
                {
                    return Redirect(confirmedIntent.NextAction.RedirectToUrl.Url);
                }

                return await HandlePaymentResult(confirmedIntent, donation.DonationId);
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe payment error");

                // Update donation status
                donation.PaymentStatus = "failed";
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, e.StripeError.Message);
                ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
                return View("Payment", model);
            }
        }

        //[Authorize(Roles = "Donor")]
        public async Task<IActionResult> PaymentConfirmation(int donationId)
        {
            var donation = await _context.Donations.FindAsync(donationId);
            if (donation == null)
            {
                return NotFound();
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(donation.StripePaymentIntentId);

            return await HandlePaymentResult(paymentIntent, donationId);
        }

        private async Task<IActionResult> HandlePaymentResult(PaymentIntent paymentIntent, int donationId)
        {
            var donation = await _context.Donations.FindAsync(donationId);
            if (donation == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(donation.DonorId);

            switch (paymentIntent.Status)
            {
                case "succeeded":
                    donation.PaymentStatus = "succeeded";

                    // Get the latest charge (correct way to access charges in newer Stripe API versions)
                    var chargeService = new ChargeService();
                    var charges = chargeService.List(new ChargeListOptions
                    {
                        PaymentIntent = paymentIntent.Id
                    });

                    donation.ReceiptUrl = charges.FirstOrDefault()?.ReceiptUrl;
                    donation.StripeCustomerId = paymentIntent.CustomerId;

                    // Update user's total donations with proper type conversion
                    user.AmountDonated = (user.AmountDonated) + (double)donation.Amount;

                    await _context.SaveChangesAsync();
                    await _userManager.UpdateAsync(user);

                    return RedirectToAction("PaymentSuccess", new { donationId = donation.DonationId });

                    await _context.SaveChangesAsync();
                    await _userManager.UpdateAsync(user);

                    return RedirectToAction("PaymentSuccess", new { donationId = donation.DonationId });

                case "processing":
                    donation.PaymentStatus = "processing";
                    await _context.SaveChangesAsync();
                    return View("PaymentProcessing", donation);

                case "requires_payment_method":
                    donation.PaymentStatus = "failed";
                    await _context.SaveChangesAsync();
                    ModelState.AddModelError(string.Empty, "Payment failed. Please try another payment method.");
                    ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
                    return View("Payment", new PaymentViewModel
                    {
                        Email = user.Email,
                        Name = user.FullName
                    });

                default:
                    donation.PaymentStatus = "failed";
                    await _context.SaveChangesAsync();
                    return RedirectToAction("PaymentFailed", new { donationId = donation.DonationId });
            }
        }

       // [Authorize(Roles = "Donor")]
        public async Task<IActionResult> PaymentSuccess(int donationId)
        {
            var donation = await _context.Donations
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.DonationId == donationId);

            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

      //  [Authorize(Roles = "Donor")]
        public async Task<IActionResult> PaymentFailed(int donationId)
        {
            var donation = await _context.Donations
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.DonationId == donationId);

            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

       // [Authorize(Roles = "Donor")]
        public async Task<IActionResult> PaymentProcessing(int donationId)
        {
            var donation = await _context.Donations
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.DonationId == donationId);

            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }
    }

}