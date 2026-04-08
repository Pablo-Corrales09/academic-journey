using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HotelCarga.HotelCarga.DbModel.Entities;

namespace HotelCargaJsonRepositoryModel;

public class JsonDataContext
{
    private readonly string _jsonFolderPath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    public IReadOnlyList<booking> bookings { get; }
    public IReadOnlyList<booking_history> booking_histories { get; }
    public IReadOnlyList<booking_status> booking_statuses { get; }
    public IReadOnlyList<customer> customers { get; }
    public IReadOnlyList<queue_status> queue_statuses { get; }
    public IReadOnlyList<role> roles { get; }
    public IReadOnlyList<room> rooms { get; }
    public IReadOnlyList<room_availability> room_availabilities { get; }
    public IReadOnlyList<room_category> room_categories { get; }
    public IReadOnlyList<room_status> room_statuses { get; }
    public IReadOnlyList<user> users { get; }
    public IReadOnlyList<user_status> user_statuses { get; }
    public IReadOnlyList<waiting_queue> waiting_queues { get; }

    public booking? GetBookingById(uint id) => bookings.FirstOrDefault(b => b.id == id);
    public booking_status? GetBookingStatusById(byte id) => booking_statuses.FirstOrDefault(bs => bs.id == id);
    public queue_status? GetQueueStatusById(byte id) => queue_statuses.FirstOrDefault(qs => qs.id == id);
    public room_category? GetRoomCategoryById(byte id) => room_categories.FirstOrDefault(rc => rc.id == id);
    public room_status? GetRoomStatusById(byte id) => room_statuses.FirstOrDefault(rs => rs.id == id);
    public role? GetRoleById(uint id) => roles.FirstOrDefault(r => r.id == id);
    public user_status? GetUserStatusById(byte id) => user_statuses.FirstOrDefault(us => us.id == id);
    public room? GetRoomById(uint id) => rooms.FirstOrDefault(r => r.id == id);
    public customer? GetCustomerById(uint id) => customers.FirstOrDefault(c => c.id == id);
    public user? GetUserById(uint id) => users.FirstOrDefault(u => u.id == id);

    public JsonDataContext(string jsonFolderPath)
    {
        if (string.IsNullOrWhiteSpace(jsonFolderPath))
        {
            throw new ArgumentException("JSON folder path must be provided.", nameof(jsonFolderPath));
        }

        _jsonFolderPath = Path.GetFullPath(jsonFolderPath);

        bookings = LoadData<booking>("booking.json");
        booking_histories = LoadData<booking_history>("booking_history.json");
        booking_statuses = LoadData<booking_status>("booking_status.json");
        customers = LoadData<customer>("customer.json");
        queue_statuses = LoadData<queue_status>("queue_status.json");
        roles = LoadData<role>("role.json");
        rooms = LoadData<room>("room.json");
        room_availabilities = LoadData<room_availability>("room_availability.json");
        room_categories = LoadData<room_category>("room_category.json");
        room_statuses = LoadData<room_status>("room_status.json");
        users = LoadData<user>("user.json");
        user_statuses = LoadData<user_status>("user_status.json");
        waiting_queues = LoadData<waiting_queue>("waiting_queue.json");
    }

    private IReadOnlyList<T> LoadData<T>(string fileName)
    {
        var filePath = Path.Combine(_jsonFolderPath, fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Required JSON source file not found: {filePath}", filePath);
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
    }
}
