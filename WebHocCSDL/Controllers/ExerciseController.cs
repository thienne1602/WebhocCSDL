using Microsoft.AspNetCore.Mvc;

namespace WebHocCSDL.Controllers
{
    public class ExerciseController : Controller
    {
        public IActionResult Index()
        {
            var exercises = new List<string>
            {
                "Thiết kế CSDL cho thư viện",
                "Quản lý sinh viên",
                "Quản lý bán hàng"
            };
            return View(exercises);
        }
    }
}