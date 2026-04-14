using System;
using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.RepositoryModel.Interfaces;
using HotelCarga.RepositoryModel.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1. Cargamos la cadena de conexion de forma segura (desde user-secrets en desarrollo)
string connectionString = builder.Configuration.GetConnectionString("myConnectionString");

// Debug: Intenta acceso alternativo si el primero falla
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = builder.Configuration["ConnectionStrings:myConnectionString"];
}

// 2. Validamos que no sea nula y que NO este vacia
if (string.IsNullOrWhiteSpace(connectionString))
{
    var environment = builder.Environment.EnvironmentName;
    throw new InvalidOperationException(
        $"Connection string 'myConnectionString' not found in {environment} environment. " +
        $"Set it via: dotnet user-secrets set \"ConnectionStrings:myConnectionString\" \"<connection-string>\"");
}

// 3. Inyectamos el DbContext
builder.Services.AddDbContext<HotelCargaContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 4. Register Repository Pattern Services
RegisterRepositories(builder.Services);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void RegisterRepositories(IServiceCollection services)
{
    services.AddScoped<IUserStatusRepository, UserStatusRepository>();
    services.AddScoped<IRoomStatusRepository, RoomStatusRepository>();
    services.AddScoped<IBookingStatusRepository, BookingStatusRepository>();
    services.AddScoped<IQueueStatusRepository, QueueStatusRepository>();

    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IRoomCategoryRepository, RoomCategoryRepository>();
    services.AddScoped<IRoomAvailabilityRepository, RoomAvailabilityRepository>();
    services.AddScoped<IRoomRepository, RoomRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<IBookingRepository, BookingRepository>();
    services.AddScoped<IBookingHistoryRepository, BookingHistoryRepository>();
    services.AddScoped<IWaitingQueueRepository, WaitingQueueRepository>();

    services.AddScoped<ICustomerBookingViewRepository, CustomerBookingViewRepository>();
    services.AddScoped<IWaitingQueueReportViewRepository, WaitingQueueReportViewRepository>();
}
