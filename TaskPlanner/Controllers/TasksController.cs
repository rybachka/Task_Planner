using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;
using TaskPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; // Upewnij się, że masz tę przestrzeń nazw
using System;
using System.Threading.Tasks;
using TaskPlanner.Data;

namespace TaskPlanner.Controllers
{
    [Route("Tasks")]
    public class TasksController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<TasksController> _logger; // Logger
         private readonly ApplicationDbContext _context;

        public TasksController(IConfiguration configuration, UserManager<IdentityUser> userManager, ILogger<TasksController> logger,ApplicationDbContext context )
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger; // Inicjalizacja loggera
             _context = context;
        }

    [HttpGet("")]
    public async Task<IActionResult> Tasks()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account"); // Jeśli brak użytkownika, przekierowanie do logowania
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Wywołanie funkcji składowanej do pobrania zadań użytkownika
                var tasks = await connection.QueryAsync<TaskItem>(
                    "SELECT * FROM public.get_user_tasks(@UserId);",
                    new { UserId = Guid.Parse(userId) } // Przekazanie userId jako GUID
                );

                // Logujemy dane dla każdego taska, w tym project_id i task_id
                foreach (var task in tasks)
                {
                    Console.WriteLine($"Task ID: {task.Id}, Task Name: {task.Name}, Project ID: {task.ProjectId}, Project Name: {task.ProjectName}, Status: {task.IsCompleted}, Due Date: {task.DueDate}");
                }

                return View(tasks); // Zwrócenie wyników do widoku
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
            return StatusCode(500, "Internal server error.");
        }
    }


        [HttpGet("Details/{taskId}")]
        public async Task<IActionResult> TaskDetails(Guid taskId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Jeśli brak użytkownika, przekierowanie do logowania
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Wywołanie funkcji składowanej do pobrania szczegółów zadania
                    var taskDetails = await connection.QuerySingleAsync<TaskItem>(
                        "SELECT * FROM public.get_task_details(@TaskId);",
                        new { TaskId = taskId }
                    );

                    return View("TaskDetails", taskDetails); // Zwrócenie wyników do widoku
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
[HttpPost("CompleteTask/{taskId}")]
public async Task<IActionResult> CompleteTask(Guid taskId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");

    try
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Zmieniamy status zadania na "Completed"
            await connection.ExecuteAsync(
                "SELECT public.update_task_status(@TaskId, @IsCompleted)", 
                new { TaskId = taskId, IsCompleted = true }
            );

            // Sprawdzamy ID projektu
            var projectId = await connection.QuerySingleOrDefaultAsync<Guid>(
                "SELECT projectid FROM tasks WHERE id = @TaskId", 
                new { TaskId = taskId }
            );

            // Sprawdzamy, czy wszystkie zadania w projekcie są "Completed"
            var anyInProgress = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM tasks WHERE projectid = @ProjectId AND iscompleted = false)",
                new { ProjectId = projectId }
            );

            // Jeśli wszystkie zadania są "Completed", ustawiamy projekt na "Completed"
            if (!anyInProgress)
            {
                await connection.ExecuteAsync(
                    "UPDATE projects SET iscompleted = true WHERE id = @ProjectId", 
                    new { ProjectId = projectId }
                );
            }

            return RedirectToAction("Tasks");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error: {ex.Message}");
        return StatusCode(500, "Internal server error.");
    }
}



[HttpPost("RevertTaskCompletion/{taskId}")]
public async Task<IActionResult> RevertTaskCompletion(Guid taskId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");

    try
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "SELECT public.update_task_status(@TaskId, @IsCompleted)", 
                new { TaskId = taskId, IsCompleted = false }
            );

            // Sprawdzamy ID projektu
            var projectId = await connection.QuerySingleOrDefaultAsync<Guid>(
                "SELECT projectid FROM tasks WHERE id = @TaskId", 
                new { TaskId = taskId }
            );

            // Sprawdzamy, czy jakiekolwiek zadanie w projekcie jest w stanie "In Progress"
            var anyInProgress = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM tasks WHERE projectid = @ProjectId AND iscompleted = false)",
                new { ProjectId = projectId }
            );

            // Jeśli jakiekolwiek zadanie jest "In Progress", projekt nie może być ustawiony na "Completed"
            if (anyInProgress)
            {
                await connection.ExecuteAsync(
                    "UPDATE projects SET iscompleted = false WHERE id = @ProjectId", 
                    new { ProjectId = projectId }
                );
            }

            return RedirectToAction("Tasks");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error: {ex.Message}");
        return StatusCode(500, "Internal server error.");
    }
}




        [HttpPost("DeleteTask/{taskId}")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Log before deleting the task
                    _logger.LogInformation($"DeleteTask: Deleting task {taskId}.");

                    // Call SQL function to delete the task and update project status
                    await connection.ExecuteAsync(
                        "SELECT public.delete_task(@TaskId)", 
                        new { TaskId = taskId });

                    // Redirect to task list after deletion
                    return RedirectToAction("Tasks");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
    
[HttpPost("EditTask/{taskId}")]
public async Task<IActionResult> EditTask(Guid taskId, string description, DateTime? dueDate)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");

    try
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Wywołanie funkcji składowanej do zaktualizowania opisu i daty wykonania zadania
            await connection.ExecuteAsync(
                "SELECT public.update_task_description_and_duedate(@TaskId, @Description, @DueDate)",
                //"UPDATE tasks SET description = @Description, duedate = @DueDate WHERE id = @TaskId",
                new { TaskId = taskId, Description = description, DueDate = dueDate }
            );

            // Przekierowanie z powrotem do szczegółów zadania
            return RedirectToAction("TaskDetails", new { taskId = taskId });
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error editing task {taskId}: {ex.Message}");
        return StatusCode(500, "Internal server error.");
    }
}



    }
    
}