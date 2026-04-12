using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaContext = HotelCarga.DbModel.HotelCargaContext;
using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.DbModel;
using HotelCargaJsonRepositoryModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelCarga.ApiModel.Controllers;

[Route("[controller]")]
public class BookingController : BaseApiController
{
    public BookingController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        booking? bookingItem = null;
        if (UseJsonBackend(useJson))
        {
            bookingItem = JsonContext?.bookings.FirstOrDefault(b => b.id == id);
            if (bookingItem != null)
            {
                var status = JsonContext?.booking_statuses.FirstOrDefault(s => s.id == bookingItem.status_id);
                if (status is not null) bookingItem.status = status;

                var customer = JsonContext?.customers.FirstOrDefault(c => c.id == bookingItem.customer_id);
                if (customer is not null) bookingItem.customer = customer;

                var room = JsonContext?.rooms.FirstOrDefault(r => r.id == bookingItem.room_id);
                if (room is not null) bookingItem.room = room;
            }
        }
        else
        {
           if(DbContext is null) return DbBackendMissing();
           bookingItem = await DbContext.Set<booking>()           
            .Include(b => b.status)
            .Include(b => b.customer)
            .Include(b => b.room)
            .FirstOrDefaultAsync(b => b.id == id);
        }
        if (bookingItem is null) return NotFound();

        return Ok(BuildBookingResponse(bookingItem));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var bookings = JsonContext!.bookings;
            foreach (var bookingItem in bookings)
            {
                var status = JsonContext.booking_statuses.FirstOrDefault(s => s.id == bookingItem.status_id);
                var customer =  JsonContext.customers.FirstOrDefault(c => c.id == bookingItem.customer_id);
                var room = JsonContext.rooms.FirstOrDefault(r => r.id == bookingItem.room_id);
            }
            return Ok(bookings.Select(BuildBookingResponse).ToList());
            }
            if(DbContext is null) return DbBackendMissing();
            var entities = await DbContext.Set<booking>()
            .Include(c=> c.customer)
            .Include(c=> c.room)
            .Include(c=> c.status)
            .ToListAsync();
            return Ok(entities.Select(BuildBookingResponse).ToList());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] booking item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        await DbContext.Set<booking>().AddAsync(item);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] booking item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        DbContext.Set<booking>().Update(item);
        await DbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        var item = await DbContext.Set<booking>().FindAsync(id);
        if (item is null) return NotFound();

        DbContext.Set<booking>().Remove(item);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("GetReserveNumberById")]
    public async Task<IActionResult> GetReserveNumberById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.FirstOrDefault(b => b.id == id)?.reserve_number);
        }

        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<booking>().Where(b => b.id == id).Select(b => b.reserve_number).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetRoomIdByReserveNumber")]
    public async Task<IActionResult> GetRoomIdByReserveNumber(string reserveNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.FirstOrDefault(b => b.reserve_number == reserveNumber)?.room_id);
        }

        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<booking>().Where(b => b.reserve_number == reserveNumber).Select(b => (uint?)b.room_id).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetBookingHistoryIdsByReserveNumber")]
    public async Task<IActionResult> GetBookingHistoryIdsByReserveNumber(string reserveNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var bookingIds = JsonContext!.bookings
                .Where(b => b.reserve_number == reserveNumber)
                .Select(b => b.id)
                .ToHashSet();

            return Ok(JsonContext.booking_histories
                .Where(history => bookingIds.Contains(history.booking_id))
                .Select(history => history.id));
        }

        if (DbContext is null) return DbBackendMissing();

        var bookingId = await DbContext.Set<booking>().Where(b => b.reserve_number == reserveNumber).Select(b => (uint?)b.id).FirstOrDefaultAsync();
        if (bookingId is null) return Ok(Array.Empty<uint>());

        var result = await DbContext.Set<booking_history>().Where(h => h.booking_id == bookingId.Value).Select(h => h.id).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetOverlappingReserveNumbers")]
    public async Task<IActionResult> GetOverlappingReserveNumbers(DateTime startDate, DateTime endDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var jsonResults = JsonContext!.bookings
                .Where(b => b.check_in < endDate && b.check_out > startDate)
                .Select(b => b.reserve_number);
            return Ok(jsonResults);
        }

        if (DbContext is null) return DbBackendMissing();

        var dbResults = await DbContext.Set<booking>()
            .Where(b => b.check_in < endDate && b.check_out > startDate)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(dbResults);
    }

    [HttpGet("GetReserveNumbersByCheckInDate")]
    public async Task<IActionResult> GetReserveNumbersByCheckInDate(DateTime checkInDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings
                .Where(b => b.check_in.Date == checkInDate.Date)
                .Select(b => b.reserve_number));
        }

        if (DbContext is null) return DbBackendMissing();
        var results = await DbContext.Set<booking>()
            .Where(b => b.check_in.Date == checkInDate.Date)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(results);
    }

    [HttpGet("GetTotalPriceByReserveNumber")]
    public async Task<IActionResult> GetTotalPriceByReserveNumber(string reserveNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.FirstOrDefault(b => b.reserve_number == reserveNumber)?.total_price);
        }

        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<booking>()
            .Where(b => b.reserve_number == reserveNumber)
            .Select(b => (decimal?)b.total_price)
            .FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetReserveNumbersByStatusId")]
    public async Task<IActionResult> GetReserveNumbersByStatusId(byte statusId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.Where(b => b.status_id == statusId).Select(b => b.reserve_number));
        }

        if (DbContext is null) return DbBackendMissing();
        var results = await DbContext.Set<booking>()
            .Where(b => b.status_id == statusId)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(results);
    }

    [HttpGet("GetReserveNumbersByCustomerId")]
    public async Task<IActionResult> GetReserveNumbersByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.Where(b => b.customer_id == customerId).Select(b => b.reserve_number));
        }

        if (DbContext is null) return DbBackendMissing();
        var results = await DbContext.Set<booking>()
            .Where(b => b.customer_id == customerId)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(results);
    }

    public static object BuildBookingResponse(booking b)
    {
        return new
        {
            b.id, 
            b.reserve_number, 

            b.status_id,
            status_name = b.status?.status_name ?? "Sin estado",
                      
            b.customer_id,
            customer_name = b.customer != null
            ? $"{b.customer.first_name} {b.customer.last_name}"
            : "Sin cliente",

            b.room_id,
            room_number = b.room?.room_number ?? 0,

            b.check_in,
            b.check_out,

            b.nightly_rate,
            b.total_price
        };
    }
    
}
