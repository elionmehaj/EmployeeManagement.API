namespace EmployeeManagement.API.Models;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }

    public List<TaskItem> Tasks { get; set; } = new();
}
