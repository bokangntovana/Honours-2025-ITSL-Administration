using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class MessagesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
