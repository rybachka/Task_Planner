using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Threading.Tasks;
using TaskPlanner.Data;
using TaskPlanner.Models;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger, ApplicationDbContext dbContext,  IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;  // Wstrzykuj logger
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }



    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        Console.WriteLine("Rozpoczęcie procesu rejestracji");

        // Sprawdzenie, czy pola "Password" i "ConfirmPassword" są zgodne
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            Console.WriteLine("Błąd walidacji: Passwords do not match.");
        }
        

        if (ModelState.IsValid)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                Console.WriteLine("Rejestracja zakończona sukcesem");

                // Upewnij się, że user.Id nie jest null
                if (string.IsNullOrEmpty(user.Id))
                {
                    throw new InvalidOperationException("User ID nie zostało zainicjalizowane.");
                }

                // Dodanie wpisu do tabeli userprofile
                var userProfile = new UserProfile
                {
                    UserId = Guid.Parse(user.Id),  // Pobranie ID użytkownika z IdentityUser
                    FirstName = null,  // Na początku brak imienia
                    LastName = null    // Na początku brak nazwiska
                };

                // Dodanie wpisu do bazy danych
                _dbContext.UserProfiles.Add(userProfile);
                await _dbContext.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Home", "Home");
            }


            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Błąd rejestracji: {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        Console.WriteLine("Rozpoczęcie procesu logowania");

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Console.WriteLine("Logowanie zakończone sukcesem");
                return RedirectToAction("Home", "Home");
            }
            else
            {
                Console.WriteLine("Błąd logowania: Nieprawidłowe dane logowania");
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            }
        }
        else
        {
            Console.WriteLine("ModelState nie jest poprawny.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Błąd walidacji: {error.ErrorMessage}");
            }
        }

        // Jeśli coś poszło nie tak, zwracamy formularz z błędami
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Home", "Home");
    }




    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        // Sprawdź, czy użytkownik jest zalogowany
        if (User == null || string.IsNullOrEmpty(_userManager.GetUserId(User)))
        {
            return RedirectToAction("Login", "Account"); // Przekierowanie do logowania, jeśli brak użytkownika
        }

        Console.WriteLine("Profile method was called.");
        var userId = _userManager.GetUserId(User);
        Console.WriteLine($"User ID: {userId}");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account"); // Przekierowanie, jeśli brak UserId
        }

        // Pobierz ConnectionString
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Brak poprawnego ConnectionString w konfiguracji.");
        }

        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Pobierz dane użytkownika z funkcji składowanej
                var userProfile = await connection.QueryFirstOrDefaultAsync<UserProfile>(
                    "SELECT * FROM get_user_profile(@UserId);",
                    new { UserId = Guid.Parse(userId) } // Konwertuj userId na GUID
                );

                if (userProfile == null)
                {
                    // Jeśli brak profilu w bazie, wyświetl pusty formularz
                    userProfile = new UserProfile
                    {
                        UserId = Guid.Parse(userId), // Konwersja na string dla modelu
                        FirstName = null,
                        LastName = null
                    };
                }

                return View(userProfile);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania profilu: {ex.Message}");
            TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania profilu.";
            return RedirectToAction("Home", "Home");
        }
    }



    [HttpPost]
    public async Task<IActionResult> Profile(UserProfile model)
    {
        Console.WriteLine("Rozpoczęcie zapisu danych profilu.");

        // Pobranie ID użytkownika
        var userId = _userManager.GetUserId(User); // Metoda UserManager do uzyskania ID zalogowanego użytkownika
       
        // Sprawdzenie, czy userId jest null lub pusty
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Błąd: Użytkownik nie jest zalogowany.";
            return RedirectToAction(nameof(Profile));
        }

        try
        {
             using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            // Wykonanie funkcji składowanej
            await connection.ExecuteAsync(
                "SELECT save_user_profile(@UserId, @FirstName, @LastName)",
                new
                {
                    UserId = Guid.Parse(userId), // Bezpieczne parsowanie do GUID
                    FirstName = model.FirstName,
                    LastName = model.LastName
                }
            );

            // Ustawienie komunikatu o powodzeniu
            TempData["SuccessMessage"] = "Data was saved successfully";

            // Przekierowanie na GET Profile, aby wyświetlić zaktualizowane dane
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas zapisu danych profilu: {ex.Message}");
            TempData["ErrorMessage"] = "Wystąpił błąd podczas zapisywania danych.";
        }

        // W przypadku błędów walidacji, wyświetl widok z aktualnym modelem
        return View(model);
    }



}
