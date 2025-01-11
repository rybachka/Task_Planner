using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;
using TaskPlanner.Data;
using TaskPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Data;


[Route("Projects")]
public class ProjectController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ProjectController> _logger;
    

    public ProjectController(IConfiguration configuration, UserManager<IdentityUser> userManager, ILogger<ProjectController> logger)
    {
        _configuration = configuration;
        _userManager = userManager;
         _logger = logger; 
    }

    // Wyświetlanie listy projektów
    [HttpGet("")]  
    public async Task<IActionResult> Projects()
    {
        // Sprawdź, czy użytkownik jest zalogowany
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account"); // Przekierowanie do logowania, jeśli brak użytkownika
        }

        // Pobierz ConnectionString
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Wywołanie funkcji składowanej do pobierania projektów użytkownika
                var projects = await connection.QueryAsync<Project>(
                    "SELECT * FROM public.get_user_projects(@UserId);",
                    new { UserId = Guid.Parse(userId) } // Konwertuj UserId na GUID
                );

                return View(projects);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania projektów: {ex.Message}");
            TempData["ErrorMessage"] = "Wystąpił błąd podczas ładowania projektów.";
            return RedirectToAction("Index", "Home");
        }
    }

    // Wyświetlanie szczegółów projektu
   [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        Console.WriteLine($"Fetching details for Project ID: {id}");
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid project ID.");
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Pobierz projekt z bazy danych
                var project = await connection.QueryFirstOrDefaultAsync<Project>(
                    "SELECT * FROM projects WHERE id = @Id;", new { Id = id });

                if (project == null)
                {
                    return NotFound("Project not found.");
                }

                // Pobierz zadania przypisane do projektu
                var tasks = await connection.QueryAsync<TaskItem>(
                    "SELECT * FROM tasks WHERE projectid = @ProjectId;", new { ProjectId = id });

                project.Tasks = tasks.ToList(); // Przypisz zadania do projektu

                return View(project);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        // Możesz ustawić dane dla widoku, jeśli są wymagane (np. dla list wyboru).
        return View();
    }


 [HttpPost("Create")]
    public async Task<IActionResult> Create(Project project, List<TaskItem> tasks)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Tasks = tasks;
            return View(project);
        }

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
                var taskNames = tasks.Select(t => t.Name).ToArray();
                var taskDescriptions = tasks.Select(t => t.Description).ToArray();
                var taskDueDates = tasks.Select(t => t.DueDate).ToArray();
                await connection.ExecuteAsync(
                    "SELECT public.add_project_with_tasks(@UserId, @ProjectName, @ProjectDescription, @ProjectDueDate, @TaskNames::text[], @TaskDescriptions::text[], @TaskDueDates::date[]);",
                    new
                    {
                        UserId = Guid.Parse(userId),
                        ProjectName = project.Name,
                        ProjectDescription = project.Description,
                        ProjectDueDate = project.DueDate?.Date,
                        TaskNames = taskNames,          
                        TaskDescriptions = taskDescriptions, 
                        TaskDueDates = taskDueDates   
                    }
                );
            }
            return RedirectToAction("Projects");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas dodawania projektu i zadań: {ex.Message}");
            TempData["ErrorMessage"] = "Wystąpił błąd podczas tworzenia projektu.";
            return View(project);
        }
    }

[HttpPost("UpdateProjectCompletion")]
public async Task<IActionResult> UpdateProjectCompletion(Guid projectId)
{
    var userId = _userManager.GetUserId(User);
    if (string.IsNullOrEmpty(userId))
    {
        Console.WriteLine("User is not logged in.");
        return RedirectToAction("Login", "Account");
    }

    var connectionString = _configuration.GetConnectionString("DefaultConnection");

    try
    {
        Console.WriteLine($"Updating project status for Project ID: {projectId}, New Status: True");

        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Wywołanie funkcji składowanej, która aktualizuje status projektu i zadań
            await connection.ExecuteAsync(
                "SELECT public.update_project_and_tasks_status(@ProjectId, true);",
                new { ProjectId = projectId }
            );

            Console.WriteLine($"Successfully updated status for Project ID: {projectId}");
            return RedirectToAction("Projects");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occurred while updating project status: {ex.Message}");
        TempData["ErrorMessage"] = "Wystąpił błąd podczas aktualizowania projektu.";
        return RedirectToAction("Index", "Home");
    }
}

    [HttpPost("DeleteProject/{projectId}")]
    public async Task<IActionResult> DeleteProject(Guid projectId)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Log before deleting the project
                _logger.LogInformation($"DeleteProject: Deleting project {projectId} and its associated tasks.");

                // Call the SQL function to delete the project and its tasks
                await connection.ExecuteAsync(
                    "SELECT public.delete_project_with_tasks(@ProjectId)", 
                    new { ProjectId = projectId });

                // Redirect to projects list after deletion
                return RedirectToAction("Projects");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }




}




