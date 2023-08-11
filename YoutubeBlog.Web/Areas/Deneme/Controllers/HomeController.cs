using Microsoft.AspNetCore.Mvc;

namespace YoutubeBlog.Web.Areas.Deneme.Controllers
{
    [Area("Deneme")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
