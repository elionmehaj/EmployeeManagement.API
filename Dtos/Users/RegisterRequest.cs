namespace EmployeeManagement.API.Dtos;

public record RegisterRequest(string FullName, string Email, string Password, string Role);
