using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelCarga.Models.Rooms;

public class RoomFiltersViewModel
{
    [Display(Name = "Room Number")]
    public uint? RoomNumber { get; set; }

    [Display(Name = "Min Rate")]
    [Range(0, double.MaxValue)]
    public decimal? MinRate { get; set; }

    [Display(Name = "Max Rate")]
    [Range(0, double.MaxValue)]
    public decimal? MaxRate { get; set; }

    [Display(Name = "Status")]
    public byte? StatusId { get; set; }

    [Display(Name = "Category")]
    public byte? CategoryId { get; set; }

    [Display(Name = "Floor")]
    [Range(0, 255)]
    public byte? FloorNumber { get; set; }
}

public class RoomListItemViewModel
{
    public uint Id { get; init; }
    public uint RoomNumber { get; init; }
    public byte? FloorNumber { get; init; }
    public decimal NightlyRate { get; init; }
    public byte StatusId { get; init; }
    public byte CategoryId { get; init; }
    public string StatusName { get; init; } = "Unassigned";
    public string CategoryName { get; init; } = "Unassigned";
}

public class RoomIndexStatsViewModel
{
    public int TotalRooms { get; init; }
    public int MatchingRooms { get; init; }
    public int AvailableRooms { get; init; }
    public decimal AverageNightlyRate { get; init; }
}

public class RoomIndexViewModel
{
    public List<RoomListItemViewModel> Rooms { get; init; } = [];
    public RoomFiltersViewModel Filters { get; init; } = new();
    public List<SelectListItem> StatusOptions { get; init; } = [];
    public List<SelectListItem> CategoryOptions { get; init; } = [];
    public RoomIndexStatsViewModel Stats { get; init; } = new();
    public string ActiveFilterSummary { get; init; } = "Showing the full room inventory.";
}

public class RoomFormViewModel
{
    public uint Id { get; set; }

    [Required]
    [Display(Name = "Room Number")]
    [Range(1, uint.MaxValue)]
    public uint RoomNumber { get; set; }

    [Required]
    [Display(Name = "Status")]
    public byte StatusId { get; set; }

    [Required]
    [Display(Name = "Category")]
    public byte CategoryId { get; set; }

    [Required]
    [Display(Name = "Nightly Rate")]
    [Range(0.01, 999999.99, ErrorMessage = "La tarifa debe ser mayor a 0.")]
    public decimal NightlyRate { get; set; }

    [Display(Name = "Floor")]
    [Range(0, 255)]
    public byte? FloorNumber { get; set; }

    public List<SelectListItem> StatusOptions { get; set; } = [];
    public List<SelectListItem> CategoryOptions { get; set; } = [];
    public string PageTitle { get; set; } = string.Empty;
    public string IntroText { get; set; } = string.Empty;
    public string SubmitLabel { get; set; } = string.Empty;
    public string HeroEyebrow { get; set; } = string.Empty;
}

public class RoomBookingViewModel
{
    public uint Id { get; init; }
    public string ReserveNumber { get; init; } = string.Empty;
    public uint CustomerId { get; init; }
    public string CustomerName { get; init; } = "Guest";
    public string StatusName { get; init; } = "Unknown";
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public decimal TotalPrice { get; init; }
}

public class RoomBookingHistoryViewModel
{
    public uint Id { get; init; }
    public uint BookingId { get; init; }
    public uint CustomerId { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string StatusName { get; init; } = "Unknown";
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public DateTime? CreatedAt { get; init; }
    public decimal? TotalPrice { get; init; }
}

public class RoomAvailabilityViewModel
{
    public uint Id { get; init; }
    public DateTime StartSchedule { get; init; }
    public DateTime EndSchedule { get; init; }
    public DateTime? CreatedAt { get; init; }
}

public class RoomDetailsViewModel
{
    public RoomListItemViewModel Room { get; init; } = new();
    public List<RoomBookingViewModel> Bookings { get; init; } = [];
    public List<RoomBookingHistoryViewModel> BookingHistories { get; init; } = [];
    public List<RoomAvailabilityViewModel> Availabilities { get; init; } = [];
}

public class RoomCollectionPageViewModel<TItem>
{
    public RoomListItemViewModel Room { get; init; } = new();
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<TItem> Items { get; init; } = [];
}
