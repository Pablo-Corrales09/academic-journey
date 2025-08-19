using Microsoft.AspNetCore.Mvc;

namespace Smart_Gym.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
