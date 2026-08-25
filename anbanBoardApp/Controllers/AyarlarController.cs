using Microsoft.AspNetCore.Mvc;

namespace anbanBoardApp.Controllers
{
    public class AyarlarController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}