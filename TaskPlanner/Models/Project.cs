using System;
using System.Collections.Generic;

namespace TaskPlanner.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        // Lista zadań przypisanych do projektu
        public List<TaskItem>? Tasks { get; set; }
    }
}
