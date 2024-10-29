using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskPlanner.Models;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;  // Wstrzykuj logger
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

        if (ModelState.IsValid)
        {
            Console.WriteLine("ModelState jest poprawny");
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                Console.WriteLine("Rejestracja zakończona sukcesem");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect("http://localhost:5178/index.html");;
            }

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Błąd rejestracji: {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
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
                return Redirect("http://localhost:5178/index.html");
            }

            Console.WriteLine("Błąd logowania: Nieprawidłowe dane logowania");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
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
        return RedirectToAction("index", "Home");
    }
}