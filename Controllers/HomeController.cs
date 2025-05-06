using System.Diagnostics;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITSL_Administration.Controllers
{
    public class HomeController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
       
    }
}
