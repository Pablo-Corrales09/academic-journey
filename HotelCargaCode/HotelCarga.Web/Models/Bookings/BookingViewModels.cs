using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelCarga.Models.Bookings;

public class BookingFiltersViewModel
{
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Reserve Number")]
    public string? ReserveNumber { get; set; }

    [Display(Name = "Customer")]
    public uint? CustomerId { get; set; }

    [Display(Name = "Room")]
    public uint? RoomId { get; set; }

    [Display(Name = "Status")]
    public byte? StatusId { get; set; }

    [Display(Name = "Check-In Date")]
    [DataType(DataType.Date)]
    public DateTime? CheckInDate { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}

public class BookingListItemViewModel
{
    public uint Id { get; init; }
    public uint? WaitingQueueId { get; init; }
    public bool IsQueueRequest { get; init; }
    public string ReserveNumber { get; init; } = string.Empty;
    public byte StatusId { get; init; }
    public string StatusName { get; init; } = "Unknown";
    public uint CustomerId { get; init; }
    public string CustomerName { get; init; } = "Guest";
    public uint RoomId { get; init; }
    public uint RoomNumber { get; init; }
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public decimal NightlyRate { get; init; }
    public decimal TotalPrice { get; init; }
}

public class BookingIndexStatsViewModel
{
    public int TotalBookings { get; init; }
    public int MatchingBookings { get; init; }
    public int ActiveBookings { get; init; }
    public decimal TotalRevenue { get; init; }
}

public class BookingIndexViewModel
{
    public List<BookingRoomTypeCardViewModel> RoomTypes { get; init; } = [];
    public string? SearchTerm { get; init; }
    public bool IsApiAvailable { get; init; } = true;
    public List<BookingListItemViewModel> Bookings { get; init; } = [];
    public BookingFiltersViewModel Filters { get; init; } = new();
    public List<SelectListItem> CustomerOptions { get; init; } = [];
    public List<SelectListItem> RoomOptions { get; init; } = [];
    public List<SelectListItem> StatusOptions { get; init; } = [];
    public BookingIndexStatsViewModel Stats { get; init; } = new();
    public BookingPaginationViewModel Pagination { get; init; } = new();
    public string ActiveFilterSummary { get; init; } = "Showing the full booking ledger.";
}

public class BookingPaginationViewModel
{
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}

public class BookingRoomTypeCardViewModel
{
    public string CategoryName { get; init; } = "Room";
    public string Description { get; init; } = string.Empty;
    public decimal NightlyRateFrom { get; init; }
    public decimal NightlyRateTo { get; init; }
    public int AvailableRooms { get; init; }
}

public class BookingFormViewModel
{
    public uint Id { get; set; }

    [Required]
    [Display(Name = "Customer")]
    [Range(1, uint.MaxValue)]
    public uint CustomerId { get; set; }

    [Required]
    [Display(Name = "Room")]
    [Range(1, uint.MaxValue)]
    public uint RoomId { get; set; }

    [Required]
    [Display(Name = "Check-In")]
    [DataType(DataType.Date)]
    public DateTime CheckIn { get; set; }

    [Required]
    [Display(Name = "Check-Out")]
    [DataType(DataType.Date)]
    public DateTime CheckOut { get; set; }

    public string CurrentReserveNumber { get; set; } = string.Empty;
    public string CurrentStatusName { get; set; } = string.Empty;
    public decimal CurrentNightlyRate { get; set; }
    public decimal CurrentTotalPrice { get; set; }
    public bool LockCustomerSelection { get; set; }
    public List<SelectListItem> CustomerOptions { get; set; } = [];
    public List<SelectListItem> RoomOptions { get; set; } = [];
    public string PageTitle { get; set; } = string.Empty;
    public string IntroText { get; set; } = string.Empty;
    public string SubmitLabel { get; set; } = string.Empty;
    public string HeroEyebrow { get; set; } = string.Empty;
    public string? AddCustomerReturnUrl { get; set; }
    public bool ShowNoAvailabilityPrompt { get; set; }
    public string? NoAvailabilityPromptText { get; set; }
    [ValidateNever]
    public string? AlternativeRoomData { get; set; }
    [ValidateNever]
    public InlineCustomerCreateViewModel QuickCustomer { get; set; } = new();
}

public class InlineCustomerCreateViewModel
{
    [Required]
    [Display(Name = "Linked User Id")]
    [Range(1, uint.MaxValue)]
    public uint UserId { get; set; }

    [Required]
    [Display(Name = "Document Number")]
    [StringLength(40)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First Name")]
    [StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Phone")]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Address")]
    [StringLength(180)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "City")]
    [StringLength(80)]
    public string City { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Country")]
    [StringLength(80)]
    public string Country { get; set; } = string.Empty;
}

public class BookingDetailsViewModel
{
    public BookingListItemViewModel Booking { get; init; } = new();
    public List<uint> BookingHistoryIds { get; init; } = [];
    public string StatusDescription { get; init; } = "No status description available.";
    public int StayLengthNights { get; init; }
}

public class BookingLookupResultViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string QueryLabel { get; init; } = string.Empty;
    public string QueryValue { get; init; } = string.Empty;
    public string ResultLabel { get; init; } = string.Empty;
    public BookingListItemViewModel? BookingResult { get; init; }
    public List<uint> IdListResult { get; init; } = [];
    public List<string> ReserveNumbers { get; init; } = [];
    public string? ScalarResult { get; init; }
    public decimal? DecimalResult { get; init; }
    public uint? UIntResult { get; init; }
    public string? NotFoundMessage { get; init; }
}