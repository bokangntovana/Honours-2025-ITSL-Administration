using ITSL_Administration.Models;
using ITSL_Administration.Services.Interfaces;
using ITSL_Administration.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;

        public EmailController(IEmailService emailService, UserManager<User> userManager)
        {
            _emailService = emailService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> SendEmailAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new EmailViewModel
            {
                Users = users
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(EmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<string> recipients;

                if (model.SendToAll)
                {
                    recipients = await _userManager.Users.Select(u => u.Email).ToListAsync();
                }
                else
                {
                    recipients = model.SelectedEmails ?? new List<string>();
                }

                if (recipients.Any())
                {
                    await _emailService.SendEmailToManyAsync(recipients, model.Subject!, model.Message!, model.Attachments);
                    ViewBag.Message = "Emails sent successfully!";
                }
                else
                {
                    ViewBag.Message = "No recipients selected.";
                }
            }

            var users = await _userManager.Users.ToListAsync();
            model.Users = users; // Add this line
            return View(model);
        }
    }
}
