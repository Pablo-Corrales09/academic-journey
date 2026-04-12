using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelCarga.HotelCarga.DbModel.Entities;

#nullable enable

namespace HotelCarga.RepositoryModel.Interfaces;

/// <summary>
/// Repository interface for vw_waiting_queue_report view
/// </summary>
public interface IWaitingQueueReportViewRepository
{
    /// <summary>
    /// Get a single vw_waiting_queue_report entity given an exact queue_id (uint)
    /// </summary>
    Task<vw_waiting_queue_report?> GetByQueueIdAsync(uint queueId);

    /// <summary>
    /// Get a list of vw_waiting_queue_report entities given an exact document_number (string)
    /// </summary>
    Task<IEnumerable<vw_waiting_queue_report>> GetByDocumentNumberAsync(string documentNumber);

    /// <summary>
    /// Get a list of vw_waiting_queue_report entities filtering by an exact requested_room_category (string) and an exact queue_status (string)
    /// </summary>
    Task<IEnumerable<vw_waiting_queue_report>> GetByCategoryAndStatusAsync(string requestedCategory, string queueStatus);

    /// <summary>
    /// Get a list of vw_waiting_queue_report entities where the requested_check_in date falls within a specific date range
    /// </summary>
    Task<IEnumerable<vw_waiting_queue_report>> GetByRequestedCheckInDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get a list of vw_waiting_queue_report entities performing a partial search on either the first_name or last_name properties given a search keyword
    /// </summary>
    Task<IEnumerable<vw_waiting_queue_report>> SearchByNameAsync(string searchKeyword);

    /// <summary>
    /// Get a list of vw_waiting_queue_report entities filtering by an exact current_status (string), ordered by created_at in ascending order (oldest first)
    /// </summary>
    Task<IEnumerable<vw_waiting_queue_report>> GetByCurrentStatusOldestFirstAsync(string currentStatus);
}
