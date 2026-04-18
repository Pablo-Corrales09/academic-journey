using Microsoft.AspNetCore.Mvc;

namespace ParqueoCarga.Controllers
{
    public class AutomovilesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
