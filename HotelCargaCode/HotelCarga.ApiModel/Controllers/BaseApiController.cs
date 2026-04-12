using HotelCarga.DbModel;
using HotelCargaJsonRepositoryModel;
using Microsoft.AspNetCore.Mvc;

namespace HotelCarga.ApiModel.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected readonly HotelCargaContext? DbContext;
    protected readonly JsonDataContext? JsonContext;

    protected BaseApiController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null)
    {
        DbContext = dbContext;
        JsonContext = jsonContext;
    }

    protected bool UseJsonBackend(bool useJson) => useJson && JsonContext is not null;

    protected IActionResult DbBackendMissing() =>
        StatusCode(503, "Database backend is not configured. Set a valid myConnectionString or use useJson=true.");

    protected IActionResult JsonWriteUnsupported() =>
        StatusCode(501, "JSON backend is read-only for create/update/delete operations.");
}
