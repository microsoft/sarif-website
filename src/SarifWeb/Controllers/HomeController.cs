using Microsoft.AspNetCore.Mvc;

namespace SarifWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}