using Dapper;
using System.Data;
using TaskPlanner.Models; // Upewnij się, że namespace odpowiada lokalizacji klasy Project


public class ProjectRepository
{
    private readonly IDbConnection _dbConnection;

    public ProjectRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    // Metoda do pobierania wszystkich projektów
    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        var query = "SELECT * FROM Projects";
        return await _dbConnection.QueryAsync<Project>(query);
    }

    // Metoda do dodawania projektu (przykładowa)
    public async Task AddProjectAsync(Project project)
    {
        var query = "INSERT INTO Projects (Name, Description, StartDate, EndDate) VALUES (@Name, @Description, @StartDate, @EndDate)";
        await _dbConnection.ExecuteAsync(query, project);
    }
}
