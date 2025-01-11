using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Models;

namespace TaskPlanner.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<TaskItem> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure table and column names explicitly to match the database schema
        builder.Entity<TaskItem>()
            .ToTable("tasks") // Specify the table name
            .Property(t => t.Id)
            .HasColumnName("id"); // Ensure column name matches the database
        
        builder.Entity<TaskItem>()
            .Property(t => t.Description)
            .HasColumnName("description");

        builder.Entity<TaskItem>()
            .Property(t => t.DueDate)
            .HasColumnName("duedate");

        builder.Entity<TaskItem>()
            .Property(t => t.IsCompleted)
            .HasColumnName("iscompleted");

        builder.Entity<TaskItem>()
            .Property(t => t.Name)
            .HasColumnName("name");

        builder.Entity<TaskItem>()
            .Property(t => t.ProjectId)
            .HasColumnName("projectid"); // Correct column name for ProjectId

        builder.Entity<TaskItem>()
            .Property(t => t.ProjectName)
            .HasColumnName("projectname");

        // Define relationships between entities
        builder.Entity<TaskItem>()
            .HasOne<Project>()
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    }
}
