using ITSL_Administration.Services;
using ITSL_Administration.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(EmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _emailService.SendEmailAsync(model.ToEmail, model.Subject, model.Message);
                ViewBag.Message = "Email sent successfully!";
            }
            return View(model);
        }
    }
}
