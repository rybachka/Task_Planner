using System.Data;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Data;
using TaskPlanner.Models;

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

// Rejestracja Npgsql DataSource i Mapowanie Composite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapComposite<TaskRecord>("task_record");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddSingleton(dataSource);
 // PRZENIESIONE TUTAJ

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

app.UseAuthentication(); 
app.UseHttpsRedirection();
app.UseAuthorization(); 
app.MapRazorPages();
app.MapControllers();
app.MapDefaultControllerRoute();


app.Run();
