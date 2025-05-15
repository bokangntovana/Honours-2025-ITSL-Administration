using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
