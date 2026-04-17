using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelCarga.Models.WaitingQueue;

public class WaitingQueueFiltersViewModel
{
    [Display(Name = "From")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Display(Name = "To")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [Display(Name = "Status")]
    public string StatusName { get; set; } = "PENDING";
}

public class WaitingQueueListItemViewModel
{
    public uint Id { get; init; }
    public string RequestNumber { get; init; } = "-";
    public uint CustomerId { get; init; }
    public string CustomerName { get; init; } = "Unknown";
    public byte RoomCategoryId { get; init; }
    public string RoomCategoryName { get; init; } = "Unknown";
    public byte StatusId { get; init; }
    public string StatusName { get; init; } = "PENDING";
    public DateTime RequestedCheckIn { get; init; }
    public DateTime? CheckOut { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public class WaitingQueueIndexViewModel
{
    public WaitingQueueFiltersViewModel Filters { get; init; } = new();
    public List<WaitingQueueListItemViewModel> Items { get; init; } = [];
}

public sealed class WaitingQueueApiDto
{
    public uint id { get; set; }

    [JsonPropertyName("request_number")]
    public string? request_number { get; set; }

    public uint customer_id { get; set; }

    [JsonPropertyName("customer_name")]
    public string? customer_name { get; set; }

    public byte room_category_id { get; set; }

    [JsonPropertyName("room_category_name")]
    public string? room_category_name { get; set; }

    public byte status_id { get; set; }

    [JsonPropertyName("status_name")]
    public string? status_name { get; set; }

    public DateTime requested_check_in { get; set; }

    public DateTime? check_out { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
}
