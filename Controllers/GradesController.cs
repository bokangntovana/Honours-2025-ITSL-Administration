using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class GradesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
