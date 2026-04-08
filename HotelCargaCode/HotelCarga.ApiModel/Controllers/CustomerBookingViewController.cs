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
public class CustomerBookingViewController : BaseApiController
{
    public CustomerBookingViewController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
        : base(dbContext, jsonContext)
    {
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildCustomerBookingViews());
        if (DbContext is null) return DbBackendMissing();

        var result = await (from b in DbContext.Set<booking>()
                            join c in DbContext.Set<customer>() on b.customer_id equals c.id
                            join u in DbContext.Set<user>() on c.user_id equals u.id
                            join r in DbContext.Set<room>() on b.room_id equals r.id
                            join bs in DbContext.Set<booking_status>() on b.status_id equals bs.id
                            join cat in DbContext.Set<room_category>() on r.category_id equals cat.id
                            join rs in DbContext.Set<room_status>() on r.status_id equals rs.id
                            select new vw_customer_booking
                            {
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                phone = c.phone,
                                email = u.email,
                                address = c.address,
                                city = c.city,
                                country = c.country,
                                booking_id = b.id,
                                reserve_number = b.reserve_number,
                                check_in = b.check_in,
                                check_out = b.check_out,
                                nightly_rate = b.nightly_rate,
                                total_price = b.total_price,
                                booking_status = bs.status_name,
                                room_number = r.room_number,
                                floor_number = r.floor_number,
                                room_category = cat.category_name,
                                room_status = rs.status_name,
                                booking_created_at = b.created_at,
                                booking_updated_at = b.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByReserveNumber")]
    public async Task<IActionResult> GetByReserveNumber(string reserveNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildCustomerBookingViews().FirstOrDefault(v => v.reserve_number == reserveNumber));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from b in DbContext.Set<booking>()
                            join c in DbContext.Set<customer>() on b.customer_id equals c.id
                            join u in DbContext.Set<user>() on c.user_id equals u.id
                            join r in DbContext.Set<room>() on b.room_id equals r.id
                            join bs in DbContext.Set<booking_status>() on b.status_id equals bs.id
                            join cat in DbContext.Set<room_category>() on r.category_id equals cat.id
                            join rs in DbContext.Set<room_status>() on r.status_id equals rs.id
                            where b.reserve_number == reserveNumber
                            select new vw_customer_booking
                            {
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                phone = c.phone,
                                email = u.email,
                                address = c.address,
                                city = c.city,
                                country = c.country,
                                booking_id = b.id,
                                reserve_number = b.reserve_number,
                                check_in = b.check_in,
                                check_out = b.check_out,
                                nightly_rate = b.nightly_rate,
                                total_price = b.total_price,
                                booking_status = bs.status_name,
                                room_number = r.room_number,
                                floor_number = r.floor_number,
                                room_category = cat.category_name,
                                room_status = rs.status_name,
                                booking_created_at = b.created_at,
                                booking_updated_at = b.updated_at
                            }).FirstOrDefaultAsync();
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("GetByCustomerId")]
    public async Task<IActionResult> GetByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildCustomerBookingViews().Where(v => v.customer_id == customerId));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from b in DbContext.Set<booking>()
                            join c in DbContext.Set<customer>() on b.customer_id equals c.id
                            join u in DbContext.Set<user>() on c.user_id equals u.id
                            join r in DbContext.Set<room>() on b.room_id equals r.id
                            join bs in DbContext.Set<booking_status>() on b.status_id equals bs.id
                            join cat in DbContext.Set<room_category>() on r.category_id equals cat.id
                            join rs in DbContext.Set<room_status>() on r.status_id equals rs.id
                            where b.customer_id == customerId
                            select new vw_customer_booking
                            {
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                phone = c.phone,
                                email = u.email,
                                address = c.address,
                                city = c.city,
                                country = c.country,
                                booking_id = b.id,
                                reserve_number = b.reserve_number,
                                check_in = b.check_in,
                                check_out = b.check_out,
                                nightly_rate = b.nightly_rate,
                                total_price = b.total_price,
                                booking_status = bs.status_name,
                                room_number = r.room_number,
                                floor_number = r.floor_number,
                                room_category = cat.category_name,
                                room_status = rs.status_name,
                                booking_created_at = b.created_at,
                                booking_updated_at = b.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByDocumentNumber")]
    public async Task<IActionResult> GetByDocumentNumber(string documentNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildCustomerBookingViews().Where(v => v.document_number == documentNumber));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from b in DbContext.Set<booking>()
                            join c in DbContext.Set<customer>() on b.customer_id equals c.id
                            join u in DbContext.Set<user>() on c.user_id equals u.id
                            join r in DbContext.Set<room>() on b.room_id equals r.id
                            join bs in DbContext.Set<booking_status>() on b.status_id equals bs.id
                            join cat in DbContext.Set<room_category>() on r.category_id equals cat.id
                            join rs in DbContext.Set<room_status>() on r.status_id equals rs.id
                            where c.document_number == documentNumber
                            select new vw_customer_booking
                            {
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                phone = c.phone,
                                email = u.email,
                                address = c.address,
                                city = c.city,
                                country = c.country,
                                booking_id = b.id,
                                reserve_number = b.reserve_number,
                                check_in = b.check_in,
                                check_out = b.check_out,
                                nightly_rate = b.nightly_rate,
                                total_price = b.total_price,
                                booking_status = bs.status_name,
                                room_number = r.room_number,
                                floor_number = r.floor_number,
                                room_category = cat.category_name,
                                room_status = rs.status_name,
                                booking_created_at = b.created_at,
                                booking_updated_at = b.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByBookingStatus")]
    public async Task<IActionResult> GetByBookingStatus(string bookingStatus, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildCustomerBookingViews().Where(v => v.booking_status == bookingStatus));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from b in DbContext.Set<booking>()
                            join c in DbContext.Set<customer>() on b.customer_id equals c.id
                            join u in DbContext.Set<user>() on c.user_id equals u.id
                            join r in DbContext.Set<room>() on b.room_id equals r.id
                            join bs in DbContext.Set<booking_status>() on b.status_id equals bs.id
                            join cat in DbContext.Set<room_category>() on r.category_id equals cat.id
                            join rs in DbContext.Set<room_status>() on r.status_id equals rs.id
                            where bs.status_name == bookingStatus
                            select new vw_customer_booking
                            {
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                phone = c.phone,
                                email = u.email,
                                address = c.address,
                                city = c.city,
                                country = c.country,
                                booking_id = b.id,
                                reserve_number = b.reserve_number,
                                check_in = b.check_in,
                                check_out = b.check_out,
                                nightly_rate = b.nightly_rate,
                                total_price = b.total_price,
                                booking_status = bs.status_name,
                                room_number = r.room_number,
                                floor_number = r.floor_number,
                                room_category = cat.category_name,
                                room_status = rs.status_name,
                                booking_created_at = b.created_at,
                                booking_updated_at = b.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByCheckInDateRange")]
    public async Task<IActionResult> GetByCheckInDateRange(DateTime startDate, DateTime endDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildCustomerBookingViews().Where(v => v.check_in >= startDate && v.check_in <= endDate));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from b in DbContext.Set<booking>()
                            join c in DbContext.Set<customer>() on b.customer_id equals c.id
                            join u in DbContext.Set<user>() on c.user_id equals u.id
                            join r in DbContext.Set<room>() on b.room_id equals r.id
                            join bs in DbContext.Set<booking_status>() on b.status_id equals bs.id
                            join cat in DbContext.Set<room_category>() on r.category_id equals cat.id
                            join rs in DbContext.Set<room_status>() on r.status_id equals rs.id
                            where b.check_in >= startDate && b.check_in <= endDate
                            select new vw_customer_booking
                            {
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                phone = c.phone,
                                email = u.email,
                                address = c.address,
                                city = c.city,
                                country = c.country,
                                booking_id = b.id,
                                reserve_number = b.reserve_number,
                                check_in = b.check_in,
                                check_out = b.check_out,
                                nightly_rate = b.nightly_rate,
                                total_price = b.total_price,
                                booking_status = bs.status_name,
                                room_number = r.room_number,
                                floor_number = r.floor_number,
                                room_category = cat.category_name,
                                room_status = rs.status_name,
                                booking_created_at = b.created_at,
                                booking_updated_at = b.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetByRoomNumberAndStatus")]
    public async Task<IActionResult> GetByRoomNumberAndStatus(uint roomNumber, string roomStatus, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return Ok(BuildCustomerBookingViews().Where(v => v.room_number == roomNumber && v.room_status == roomStatus));
        if (DbContext is null) return DbBackendMissing();

        var result = await (from b in DbContext.Set<booking>()
                            join c in DbContext.Set<customer>() on b.customer_id equals c.id
                            join u in DbContext.Set<user>() on c.user_id equals u.id
                            join r in DbContext.Set<room>() on b.room_id equals r.id
                            join bs in DbContext.Set<booking_status>() on b.status_id equals bs.id
                            join cat in DbContext.Set<room_category>() on r.category_id equals cat.id
                            join rs in DbContext.Set<room_status>() on r.status_id equals rs.id
                            where r.room_number == roomNumber && rs.status_name == roomStatus
                            select new vw_customer_booking
                            {
                                customer_id = c.id,
                                first_name = c.first_name,
                                last_name = c.last_name,
                                document_number = c.document_number,
                                phone = c.phone,
                                email = u.email,
                                address = c.address,
                                city = c.city,
                                country = c.country,
                                booking_id = b.id,
                                reserve_number = b.reserve_number,
                                check_in = b.check_in,
                                check_out = b.check_out,
                                nightly_rate = b.nightly_rate,
                                total_price = b.total_price,
                                booking_status = bs.status_name,
                                room_number = r.room_number,
                                floor_number = r.floor_number,
                                room_category = cat.category_name,
                                room_status = rs.status_name,
                                booking_created_at = b.created_at,
                                booking_updated_at = b.updated_at
                            }).ToListAsync();
        return Ok(result);
    }

    private IEnumerable<vw_customer_booking> BuildCustomerBookingViews()
    {
        return from b in JsonContext!.bookings
               join c in JsonContext.customers on b.customer_id equals c.id
               join u in JsonContext.users on c.user_id equals u.id
               join r in JsonContext.rooms on b.room_id equals r.id
               join bs in JsonContext.booking_statuses on b.status_id equals bs.id
               join cat in JsonContext.room_categories on r.category_id equals cat.id
               join rs in JsonContext.room_statuses on r.status_id equals rs.id
               select new vw_customer_booking
               {
                   customer_id = c.id,
                   first_name = c.first_name,
                   last_name = c.last_name,
                   document_number = c.document_number,
                   phone = c.phone,
                   email = u.email,
                   address = c.address,
                   city = c.city,
                   country = c.country,
                   booking_id = b.id,
                   reserve_number = b.reserve_number,
                   check_in = b.check_in,
                   check_out = b.check_out,
                   nightly_rate = b.nightly_rate,
                   total_price = b.total_price,
                   booking_status = bs.status_name,
                   room_number = r.room_number,
                   floor_number = r.floor_number,
                   room_category = cat.category_name,
                   room_status = rs.status_name,
                   booking_created_at = b.created_at,
                   booking_updated_at = b.updated_at
               };
    }
}
