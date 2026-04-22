using System.ComponentModel.DataAnnotations;

namespace HotelCarga.Models.Customers;

public class CustomerFiltersViewModel
{
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Display(Name = "Document")]
    public string? DocumentNumber { get; set; }

    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Country")]
    public string? Country { get; set; }

    [Display(Name = "Linked User")]
    public bool? HasLinkedUser { get; set; }
}

public class CustomerListItemViewModel
{
    public uint Id { get; init; }
    public uint? UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string Username { get; init; } = "Unassigned";
    public string Email { get; init; } = "Unassigned";
    public string RoleName { get; init; } = "Unassigned";
    public string StatusName { get; init; } = "Unassigned";
    public int BookingsCount { get; init; }
    public int WaitingQueuesCount { get; init; }
}

public class CustomerIndexStatsViewModel
{
    public int TotalCustomers { get; init; }
    public int MatchingCustomers { get; init; }
    public int LinkedUsers { get; init; }
    public int ActiveLinkedUsers { get; init; }
}

public class CustomerIndexViewModel
{
    public List<CustomerListItemViewModel> Customers { get; init; } = [];
    public CustomerFiltersViewModel Filters { get; init; } = new();
    public CustomerIndexStatsViewModel Stats { get; init; } = new();
    public string ActiveFilterSummary { get; init; } = "Showing the full customer registry.";
}

public class CustomerFormViewModel
{
    public uint Id { get; set; }

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

    public string PageTitle { get; set; } = string.Empty;
    public string IntroText { get; set; } = string.Empty;
    public string SubmitLabel { get; set; } = string.Empty;
    public string HeroEyebrow { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}

public class CustomerDetailsViewModel
{
    public CustomerListItemViewModel Customer { get; init; } = new();
    public List<uint> BookingIds { get; init; } = [];
    public List<uint> BookingHistoryIds { get; init; } = [];
    public List<uint> WaitingQueueIds { get; init; } = [];
    public List<CustomerRelatedBookingViewModel> RelatedBookings { get; init; } = [];
    public List<CustomerRelatedBookingHistoryViewModel> RelatedBookingHistories { get; init; } = [];
    public List<CustomerRelatedWaitingQueueViewModel> RelatedWaitingQueues { get; init; } = [];
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public class CustomerRelatedBookingViewModel
{
    public string ReserveNumber { get; init; } = "Reservation";
    public string StatusName { get; init; } = "Unknown";
    public string RoomLabel { get; init; } = "Room pending";
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
}

public class CustomerRelatedBookingHistoryViewModel
{
    public string ActionType { get; init; } = "Update";
    public string StatusName { get; init; } = "Unknown";
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public decimal? TotalPrice { get; init; }
    public DateTime? LoggedAt { get; init; }
}

public class CustomerRelatedWaitingQueueViewModel
{
    public string RequestNumber { get; init; } = "Queue Request";
    public string RoomCategoryName { get; init; } = "Unknown";
    public string StatusName { get; init; } = "Pending";
    public DateTime RequestedCheckIn { get; init; }
    public DateTime? CheckOut { get; init; }
}

public class CustomerLookupResultViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string QueryLabel { get; init; } = string.Empty;
    public string QueryValue { get; init; } = string.Empty;
    public string ResultLabel { get; init; } = string.Empty;
    public CustomerListItemViewModel? CustomerResult { get; init; }
    public List<CustomerListItemViewModel> CustomerListResult { get; init; } = [];
    public List<uint> IdListResult { get; init; } = [];
    public string? NotFoundMessage { get; init; }
}