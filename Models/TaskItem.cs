namespace EmployeeManagement.API.Models;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = "Todo"; // Todo, InProgress, Done

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
}
