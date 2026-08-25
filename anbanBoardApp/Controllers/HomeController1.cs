using Microsoft.AspNetCore.Mvc;

namespace anbanBoardApp.Controllers
{
    public class HomeController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
