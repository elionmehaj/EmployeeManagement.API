using System.ComponentModel.DataAnnotations;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Dtos.Users;

public class UpdateUserDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "Employee";

    // opsionale: nëse do ta ndryshosh password-in
    [MinLength(6), MaxLength(200)]
    public string? Password { get; set; }

    public Department? Department { get; set; }
}
