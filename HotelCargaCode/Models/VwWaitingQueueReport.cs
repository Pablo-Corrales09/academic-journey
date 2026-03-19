using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

public partial class VwWaitingQueueReport
{
    public uint QueueId { get; set; }

    public uint CustomerId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string DocumentNumber { get; set; }

    public string RequestedRoomCategory { get; set; }

    public DateTime RequestedCheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public string QueueStatus { get; set; }

    public string CurrentStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
