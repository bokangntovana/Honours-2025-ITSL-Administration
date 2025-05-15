using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
