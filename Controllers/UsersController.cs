using EmployeeManagement.API.Data;
using EmployeeManagement.API.Dtos.Users;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; //

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] //
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    private int? GetCurrentUserId()
    {
        var idValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        return int.TryParse(idValue, out var id) ? id : null;
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    [HttpGet("me")]
    public async Task<ActionResult<UserResponseDto>> Me()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(); 

        var me = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId.Value)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Department = u.Department.ToString()
            })
            .FirstOrDefaultAsync();

        if (me == null) return NotFound();
        return Ok(me);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")] 
    public async Task<ActionResult<List<UserResponseDto>>> GetAll()
    {
        var users = await _db.Users
            .AsNoTracking()
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Department = u.Department.ToString()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("department/{department}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserResponseDto>>> GetByDepartment(Department department)
    {
        var users = await _db.Users
            .AsNoTracking()
            .Where(u => u.Department == department)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Department = u.Department.ToString()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(); 

        if (!IsAdmin() && id != currentUserId.Value)
            return Forbid(); 

        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Department = u.Department.ToString()
            })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] 
    public async Task<ActionResult<UserResponseDto>> Create(CreateUserDto dto)
    {
        if (dto == null) return BadRequest("Invalid payload.");

        var emailNormalized = dto.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(emailNormalized))
            return BadRequest("Email is required."); 

        var emailExists = await _db.Users.AnyAsync(u => u.Email == emailNormalized);
        if (emailExists) return BadRequest("Email already exists.");

        var user = new User
        {
            FullName = dto.FullName?.Trim() ?? "",
            Email = emailNormalized,
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Employee" : dto.Role.Trim(),
            Department = dto.Department
        };

        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Department = user.Department.ToString()
        };

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(); 

        if (!IsAdmin() && id != currentUserId.Value)
            return Forbid();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var emailNormalized = dto.Email.Trim().ToLowerInvariant();
        var emailTakenByOther = await _db.Users.AnyAsync(u => u.Email == emailNormalized && u.Id != id);
        if (emailTakenByOther) return BadRequest("Email already exists.");

        user.FullName = dto.FullName.Trim();
        user.Email = emailNormalized;

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            user.Role = dto.Role.Trim();
        }

        if (dto.Department.HasValue)
        {
            user.Department = dto.Department.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
