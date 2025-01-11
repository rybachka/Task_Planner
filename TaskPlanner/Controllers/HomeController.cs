using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using TaskPlanner.Models; // Assuming this contains your Project and TaskItem models

namespace TaskPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        [HttpGet("Home")]
        public async Task<IActionResult> Home()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var projects = await connection.QueryAsync<Project>(
                        "SELECT * FROM public.get_user_projects(@UserId);",
                        new { UserId = Guid.Parse(userId) }
                    );
                    var tasks = await connection.QueryAsync<TaskItem>(
                        "SELECT * FROM public.get_user_tasks(@UserId);",
                        new { UserId = Guid.Parse(userId) }
                    );
                    var tasksForToday = await connection.QueryAsync<TaskItem>(
                        "SELECT * FROM public.get_user_tasks_for_today(@UserId, @Today)",
                        new { UserId = Guid.Parse(userId), Today = DateTime.Today }
                    );
                    ViewBag.Projects = projects;
                    ViewBag.Tasks = tasks;
                    ViewBag.TasksForToday = tasksForToday;

                    return View();
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }

        }
    }
}
