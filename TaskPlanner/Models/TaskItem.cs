namespace TaskPlanner.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }

        // Użyj tych właściwości w widoku
        public string? Name { get; set; }  // Upewnij się, że właściwości 'Name' i 'Description' istnieją w tej klasie
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ProjectName { get; set; }  // Opcjonalne, jeśli potrzebujesz nazwy projektu
        public Guid ProjectId { get; set; }
    }
}
