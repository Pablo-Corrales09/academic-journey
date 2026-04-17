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
        private readonly IRoomCategoryApiService _roomCategoryService;

        public HomeController(IRoomApiService roomService, IRoomCategoryApiService roomCategoryService)
        {
            _roomService = roomService;
            _roomCategoryService = roomCategoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["LandingNav"] = true;
            return View("Index", new HomeIndexViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> RoomShowcase()
        {
            try
            {
                var roomsTask = _roomService.GetAllAsync();
                var categoriesTask = _roomCategoryService.GetAllAsync();
                await Task.WhenAll(roomsTask, categoriesTask);

                var categoryLookup = categoriesTask.Result.ToDictionary(category => category.id);
                var roomCards = roomsTask.Result
                    .Where(room => room.status_id == 1)
                    .GroupBy(room => room.category_id)
                    .Select(group =>
                    {
                        categoryLookup.TryGetValue(group.Key, out var category);
                        var categoryName = category?.category_name ?? group.First().category_name ?? "Standard";
                        var amenities = BuildAmenities(categoryName, category?.amenities);

                        return new HomeRoomCategoryCardViewModel
                        {
                            CategoryName = categoryName,
                            Description = BuildCategoryDescription(categoryName, category?.description, amenities),
                            ImageUrl = ResolveCategoryImage(categoryName),
                            NightlyRateFrom = group.Min(room => room.nightly_rate),
                            NightlyRateTo = group.Max(room => room.nightly_rate),
                            AvailableRooms = group.Count(),
                            Amenities = amenities,
                            AvailabilityLabel = BuildAvailabilityLabel(group.Count())
                        };
                    })
                    .OrderBy(card => card.CategoryName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return Json(new { isApiAvailable = true, roomCategories = roomCards });
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    isApiAvailable = false,
                    message = "No fue posible cargar las habitaciones en este momento."
                });
            }
        }

        public IActionResult About()
        {
            return View();
        }

        private static string BuildCategoryDescription(string categoryName, string? rawDescription, IReadOnlyList<string> amenities)
        {
            var amenityPreview = amenities.Count == 0
                ? "comodidades esenciales y una atmosfera serena"
                : string.Join(", ", amenities.Take(3)).ToLowerInvariant();

            if (categoryName.Contains("suite", StringComparison.OrdinalIgnoreCase))
            {
                return $"Suites pensadas para una experiencia premium, con amplitud, privacidad y detalles que elevan cada noche con {amenityPreview}.";
            }

            if (categoryName.Contains("deluxe", StringComparison.OrdinalIgnoreCase))
            {
                return $"Una propuesta superior para viajeros que valoran estilo, descanso profundo y un entorno refinado con {amenityPreview}.";
            }

            if (categoryName.Contains("family", StringComparison.OrdinalIgnoreCase) || categoryName.Contains("familiar", StringComparison.OrdinalIgnoreCase))
            {
                return $"Espacios comodos para compartir en grupo, con distribucion funcional y servicios que hacen mas simple la estancia, como {amenityPreview}.";
            }

            if (!string.IsNullOrWhiteSpace(rawDescription))
            {
                return $"{ToSentenceCase(rawDescription.Trim())}.  \nDisfrute una estadia equilibrada con {amenityPreview}.";
            }

            return $"Habitaciones disenadas para descansar con estilo, integrar productividad y disfrutar una experiencia consistente con {amenityPreview}.";
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

        private static List<string> BuildAmenities(string categoryName, string? rawAmenities)
        {
            var amenities = (rawAmenities ?? string.Empty)
                .Split(new[] { ',', ';', '|', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(MapAmenityLabel)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (amenities.Count > 0)
            {
                return amenities;
            }

            if (categoryName.Contains("suite", StringComparison.OrdinalIgnoreCase))
            {
                return ["Sala lounge", "Wi-Fi premium", "Room service", "Vista panoramica"];
            }

            if (categoryName.Contains("deluxe", StringComparison.OrdinalIgnoreCase))
            {
                return ["Wi-Fi de alta velocidad", "Aire acondicionado", "Smart TV", "Desayuno incluido"];
            }

            if (categoryName.Contains("family", StringComparison.OrdinalIgnoreCase) || categoryName.Contains("familiar", StringComparison.OrdinalIgnoreCase))
            {
                return ["Capacidad familiar", "Wi-Fi estable", "Espacio ampliado", "Check-in agilizado"];
            }

            return ["Wi-Fi", "Climatizacion", "Descanso premium"];
        }

        private static string MapAmenityLabel(string value)
        {
            var normalized = value.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            if (normalized.Contains("wifi", StringComparison.OrdinalIgnoreCase)) return "Wi‑Fi";
            if (normalized.Contains("air", StringComparison.OrdinalIgnoreCase) || normalized.Contains("a/c", StringComparison.OrdinalIgnoreCase) || normalized.Contains("ac", StringComparison.OrdinalIgnoreCase)) return "Aire acondicionado";
            if (normalized.Contains("tv", StringComparison.OrdinalIgnoreCase)) return "Smart TV";
            if (normalized.Contains("breakfast", StringComparison.OrdinalIgnoreCase) || normalized.Contains("desay", StringComparison.OrdinalIgnoreCase)) return "Desayuno incluido";
            if (normalized.Contains("room service", StringComparison.OrdinalIgnoreCase)) return "Room service";
            if (normalized.Contains("minibar", StringComparison.OrdinalIgnoreCase)) return "Minibar";
            if (normalized.Contains("balcony", StringComparison.OrdinalIgnoreCase) || normalized.Contains("balcon", StringComparison.OrdinalIgnoreCase)) return "Balcon privado";
            if (normalized.Contains("view", StringComparison.OrdinalIgnoreCase) || normalized.Contains("vista", StringComparison.OrdinalIgnoreCase)) return "Vista preferencial";
            return ToSentenceCase(normalized);
        }

        private static string BuildAvailabilityLabel(int availableRooms)
        {
            return availableRooms == 1 ? "1 habitacion disponible" : $"{availableRooms} habitaciones disponibles";
        }

        private static string ToSentenceCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return char.ToUpperInvariant(value[0]) + value[1..];
        }
    }
}