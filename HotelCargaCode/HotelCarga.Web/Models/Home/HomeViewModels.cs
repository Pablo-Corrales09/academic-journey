namespace HotelCarga.Web.Models.Home;

public class HomeIndexViewModel
{
    public bool IsApiAvailable { get; init; }
    public List<HomeRoomCategoryCardViewModel> RoomCategories { get; init; } = [];
}

public class HomeRoomCategoryCardViewModel
{
    public string CategoryName { get; init; } = "Standard";
    public string Description { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public decimal NightlyRateFrom { get; init; }
    public decimal NightlyRateTo { get; init; }
    public int AvailableRooms { get; init; }
    public string AvailabilityLabel { get; init; } = string.Empty;
    public List<string> Amenities { get; init; } = [];
}
