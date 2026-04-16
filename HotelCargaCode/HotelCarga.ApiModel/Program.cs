using System.IO;
using HotelCarga.DbModel;
using HotelCargaJsonRepositoryModel;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Explicitly add user secrets support
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var connectionString = builder.Configuration.GetConnectionString("myConnectionString");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<HotelCargaContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}

var jsonDataFolder = builder.Configuration["JsonFolder"] ?? Path.Combine(builder.Environment.ContentRootPath, "..", "sql");
builder.Services.AddSingleton(new JsonDataContext(jsonDataFolder));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register booking alert service for FIFO notifications
builder.Services.AddScoped<HotelCarga.ApiModel.Services.BookingAlertService>();

// Register background services for room availability checks and queue expiration
builder.Services.AddHostedService<HotelCarga.ApiModel.Services.RoomAvailabilitySchedulerService>();
builder.Services.AddHostedService<HotelCarga.ApiModel.Services.WaitingQueueExpirationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelCarga.ApiModel v1"));

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();

app.MapControllers();

app.Run();
