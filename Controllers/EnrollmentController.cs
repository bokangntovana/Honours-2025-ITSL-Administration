using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class EnrollmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
