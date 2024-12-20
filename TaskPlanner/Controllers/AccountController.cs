using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using TaskPlanner.Models;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
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

        // Check if Password and ConfirmPassword are the same
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            Console.WriteLine("Błąd walidacji: Passwords do not match.");
        }

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                Console.WriteLine("Rejestracja zakończona sukcesem");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");
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

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Profile()
    {
        var user = _userManager.GetUserAsync(User).Result;

        if (user == null)
        {
            Console.WriteLine("Użytkownik nie jest zalogowany.");
            return RedirectToAction("Login", "Account");
        }

        var userProfile = new
        {
            FirstName = "Mariia", // Example data (add to ApplicationUser if you want to store it)
            LastName = "Rybak",  // Example data
            Email = user.Email,
            ProfilePicturePath = user.ProfilePicturePath ?? "/uploads/default-profile.png"
        };

        return View(userProfile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
    {
        if (profilePicture != null && profilePicture.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePicture.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("Błąd: Użytkownik nie istnieje.");
                return RedirectToAction("Login", "Account");
            }

            user.ProfilePicturePath = "/uploads/" + uniqueFileName;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Profile");
        }

        Console.WriteLine("Błąd: Nie wybrano pliku.");
        return RedirectToAction("Profile");
    }
}
