using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class TutorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
