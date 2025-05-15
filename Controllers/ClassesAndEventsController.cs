using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class ClassesAndEventsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
