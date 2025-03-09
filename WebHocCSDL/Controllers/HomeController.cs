using Microsoft.AspNetCore.Mvc;

namespace WebHocCSDL.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}