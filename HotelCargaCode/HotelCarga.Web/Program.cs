using System;
using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.RepositoryModel.Interfaces;
using HotelCarga.RepositoryModel.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.Configure<HotelCarga.Web.Services.ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddScoped<HotelCarga.Web.Services.IRoomApiService, HotelCarga.Web.Services.RoomApiService>();
builder.Services.AddScoped<HotelCarga.Web.Services.IRoomCategoryApiService, HotelCarga.Web.Services.RoomCategoryApiService>();
builder.Services.AddScoped<HotelCarga.Web.Services.IUserApiService, HotelCarga.Web.Services.UserApiService>();
builder.Services.AddScoped<HotelCarga.Web.Services.ICustomerApiService, HotelCarga.Web.Services.CustomerApiService>();
builder.Services.AddScoped<HotelCarga.Web.Services.IBookingApiService, HotelCarga.Web.Services.BookingApiService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/User/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


