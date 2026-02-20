using Microsoft.AspNetCore.Mvc;

namespace CoursesApplication.Web.Controllers
{
    public class Author1Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
