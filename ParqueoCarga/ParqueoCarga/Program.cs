using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ParqueoCarga.Configuration;
using ParqueoCarga.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.AddHttpClient<IPrqAutomovilesApiService, PrqAutomovilesApiService>((serviceProvider, client) =>
{
    var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(apiSettings.BaseUrl)
        ? "http://localhost/"
        : apiSettings.BaseUrl;

    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient<IPrqParqueosApiService, PrqParqueosApiService>((serviceProvider, client) =>
{
    var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(apiSettings.BaseUrl)
        ? "http://localhost/"
        : apiSettings.BaseUrl;

    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient<IPrqIngresosAutomovilApiService, PrqIngresosAutomovilApiService>((serviceProvider, client) =>
{
    var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(apiSettings.BaseUrl)
        ? "http://localhost/"
        : apiSettings.BaseUrl;

    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/", () => Results.Redirect("/automoviles"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Automoviles}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
