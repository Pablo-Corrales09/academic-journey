using HotelCarga.Web.Models.Home;
using HotelCarga.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelCarga.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IRoomApiService _roomService;

        public HomeController(IRoomApiService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var rooms = await _roomService.GetAllAsync();
                var categories = rooms
                    .Where(room => room.status_id == 1)
                    .GroupBy(room => room.category_name ?? "Standard")
                    .Select(group => new HomeRoomCategoryCardViewModel
                    {
                        CategoryName = group.Key,
                        Description = BuildCategoryDescription(group.Key),
                        ImageUrl = ResolveCategoryImage(group.Key),
                        NightlyRateFrom = group.Min(room => room.nightly_rate),
                        NightlyRateTo = group.Max(room => room.nightly_rate),
                        AvailableRooms = group.Count()
                    })
                    .OrderBy(card => card.CategoryName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                ViewData["LandingNav"] = true;
                return View("Index", new HomeIndexViewModel
                {
                    IsApiAvailable = true,
                    RoomCategories = categories
                });
            }
            catch (HttpRequestException)
            {
                ViewData["LandingNav"] = true;
                return View("Index", new HomeIndexViewModel
                {
                    IsApiAvailable = false
                });
            }
        }

        public IActionResult About()
        {
            return View();
        }

        private static string BuildCategoryDescription(string categoryName)
        {
            if (categoryName.Contains("suite", StringComparison.OrdinalIgnoreCase))
            {
                return "Premium suites with lounge zones, panoramic views, and elevated privacy for executive stays.";
            }

            if (categoryName.Contains("deluxe", StringComparison.OrdinalIgnoreCase))
            {
                return "Deluxe rooms blending comfort and style with refined finishes for business and leisure travelers.";
            }

            if (categoryName.Contains("family", StringComparison.OrdinalIgnoreCase) || categoryName.Contains("familiar", StringComparison.OrdinalIgnoreCase))
            {
                return "Family-focused spaces designed for multi-guest stays, featuring practical layouts and extra comfort.";
            }

            return "Thoughtfully designed accommodations with balanced comfort, quality rest, and modern essentials.";
        }

        private static string ResolveCategoryImage(string categoryName)
        {
            if (categoryName.Contains("suite", StringComparison.OrdinalIgnoreCase))
            {
                return "https://images.unsplash.com/photo-1590490360182-c33d57733427?auto=format&fit=crop&w=1400&q=80";
            }

            if (categoryName.Contains("deluxe", StringComparison.OrdinalIgnoreCase))
            {
                return "https://images.unsplash.com/photo-1611892440504-42a792e24d32?auto=format&fit=crop&w=1400&q=80";
            }

            if (categoryName.Contains("family", StringComparison.OrdinalIgnoreCase) || categoryName.Contains("familiar", StringComparison.OrdinalIgnoreCase))
            {
                return "https://images.unsplash.com/photo-1566669437685-bfe5e38af1f5?auto=format&fit=crop&w=1400&q=80";
            }

            return "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?auto=format&fit=crop&w=1400&q=80";
        }
    }
}