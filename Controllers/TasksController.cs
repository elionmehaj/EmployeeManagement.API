using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskItem>>> GetAll([FromQuery] int? projectId)
    {
        var query = _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .AsNoTracking()
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        var tasks = await query.ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return NotFound();
        return Ok(task);
    }

    [Authorize(Roles = "Admin")] 
    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(TaskItem task)
    {
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == task.ProjectId);
        if (!projectExists) return BadRequest("ProjectId is invalid.");

        if (task.AssignedUserId.HasValue)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == task.AssignedUserId.Value);
            if (!userExists) return BadRequest("AssignedUserId is invalid.");
        }

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [Authorize(Roles = "Admin")] 
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaskItem task)
    {
        if (id != task.Id) return BadRequest("Id mismatch");

        var exists = await _db.Tasks.AnyAsync(t => t.Id == id);
        if (!exists) return NotFound();

        var projectExists = await _db.Projects.AnyAsync(p => p.Id == task.ProjectId);
        if (!projectExists) return BadRequest("ProjectId is invalid.");

        if (task.AssignedUserId.HasValue)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == task.AssignedUserId.Value);
            if (!userExists) return BadRequest("AssignedUserId is invalid.");
        }

        _db.Entry(task).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Admin")] 
    [HttpPatch("{id:int}/assign/{userId:int}")]
    public async Task<IActionResult> Assign(int id, int userId)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists) return BadRequest("User does not exist.");

        task.AssignedUserId = userId;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Admin")] 
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
