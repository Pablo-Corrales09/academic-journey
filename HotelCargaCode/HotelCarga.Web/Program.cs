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

// 1. Cargamos la cadena de conexión de forma segura (desde user-secrets en desarrollo)
string connectionString = builder.Configuration.GetConnectionString("myConnectionString");

// Debug: Intenta acceso alternativo si el primero falla
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = builder.Configuration["ConnectionStrings:myConnectionString"];
}

// 2. Validamos que no sea nula y que NO esté vacía
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

// Add services to the container.
builder.Services.AddControllersWithViews();

// ¡SOLO UN builder.Build()!
var app = builder.Build();

// ============================================================
// 🛠️ BLOQUE DE PRUEBA DE CONEXIÓN A LA BASE DE DATOS
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HotelCargaContext>();
        
        Console.WriteLine("Intentando conectar a la base de datos...");
        
        bool isConnected = await context.Database.CanConnectAsync();

        if (isConnected)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====================================================");
            Console.WriteLine("✅ ¡ÉXITO! Conexión a MySQL en Azure establecida.");
            Console.WriteLine("====================================================");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("====================================================");
            Console.WriteLine("⚠️ ADVERTENCIA: No se pudo conectar a la base de datos.");
            Console.WriteLine("====================================================");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("====================================================");
        Console.WriteLine($"❌ EXCEPCIÓN AL CONECTAR: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"🔍 Detalle interno: {ex.InnerException.Message}");
        }
        Console.WriteLine("====================================================");
        Console.ResetColor();
    }
}
// ============================================================
// FIN DEL BLOQUE DE PRUEBA
// ============================================================

// Configure the HTTP request pipeline (Limpio y sin duplicados)
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

// ============================================================
// Repository Pattern Dependency Injection Extension
// ============================================================
static void RegisterRepositories(IServiceCollection services)
{
    // Status Repositories
    services.AddScoped<IUserStatusRepository, UserStatusRepository>();
    services.AddScoped<IRoomStatusRepository, RoomStatusRepository>();
    services.AddScoped<IBookingStatusRepository, BookingStatusRepository>();
    services.AddScoped<IQueueStatusRepository, QueueStatusRepository>();

    // Entity Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IRoomCategoryRepository, RoomCategoryRepository>();
    services.AddScoped<IRoomAvailabilityRepository, RoomAvailabilityRepository>();
    services.AddScoped<IRoomRepository, RoomRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<IBookingRepository, BookingRepository>();
    services.AddScoped<IBookingHistoryRepository, BookingHistoryRepository>();
    services.AddScoped<IWaitingQueueRepository, WaitingQueueRepository>();

    // View Repositories
    services.AddScoped<ICustomerBookingViewRepository, CustomerBookingViewRepository>();
    services.AddScoped<IWaitingQueueReportViewRepository, WaitingQueueReportViewRepository>();
}