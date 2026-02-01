using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Dtos.Users;

public class CreateUserDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    // password plain vetëm për krijim; ne e ruajmë si hash
    [Required, MinLength(6), MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "Employee";
}
