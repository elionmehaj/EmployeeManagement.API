using EmployeeManagement.API.Data;
using EmployeeManagement.API.Dtos.Users;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/users
    [HttpGet]
    public async Task<ActionResult<List<UserResponseDto>>> GetAll()
    {
        var users = await _db.Users
            .AsNoTracking()
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET /api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role
            })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();
        return Ok(user);
    }

    // POST /api/users
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create(CreateUserDto dto)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists) return BadRequest("Email already exists.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Employee" : dto.Role.Trim()
        };

        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    // PUT /api/users/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var emailNormalized = dto.Email.Trim().ToLowerInvariant();
        var emailTakenByOther = await _db.Users.AnyAsync(u => u.Email == emailNormalized && u.Id != id);
        if (emailTakenByOther) return BadRequest("Email already exists.");

        user.FullName = dto.FullName.Trim();
        user.Email = emailNormalized;
        user.Role = string.IsNullOrWhiteSpace(dto.Role) ? user.Role : dto.Role.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/users/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Opsionale: para fshirjes, mund të "unassign" tasks.
        // Por ti ke OnDelete(SetNull) te AppDbContext, kështu që është OK.

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
