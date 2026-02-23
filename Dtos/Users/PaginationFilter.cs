namespace EmployeeManagement.API.Dtos.Users;

public record PaginationFilter
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}
