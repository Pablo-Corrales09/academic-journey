using System;
using HotelCarga.DbModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Load connection string from user secrets (recommended) or environment
var connectionString = builder.Configuration["myConnectionString"]
    ?? builder.Configuration.GetSection("ConnectionStrings")["myConnectionString"]
    ?? throw new InvalidOperationException("myConnectionString is not configured. Use dotnet user-secrets set \"myConnectionString\" \"<your-connection-string>\"");

builder.Services.AddDbContext<HotelCargaContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
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