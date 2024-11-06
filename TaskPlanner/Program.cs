using System.Data;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Data;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja połączenia z bazą danych PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfiguracja Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Dodanie usług do kontenera
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Konfiguracja połączenia Npgsql
builder.Services.AddTransient<IDbConnection>((sp) =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Rejestracja repozytorium
builder.Services.AddTransient<ProjectRepository>();

// Dodanie widoków i plików statycznych
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();


// Dodanie plików statycznych i routingu
app.UseStaticFiles();
app.UseRouting();

// Konfiguracja potoku żądań HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // Obsługa logowania
app.UseHttpsRedirection();
app.UseAuthorization();  // Obsługa autoryzacji
app.MapRazorPages();
app.MapControllers();
app.MapDefaultControllerRoute();





// Przykładowy kod do testowania
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
