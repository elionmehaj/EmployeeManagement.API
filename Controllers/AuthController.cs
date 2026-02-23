using EmployeeManagement.API.Data;
using EmployeeManagement.API.Dtos;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Authorization; // ✅ SHTUAR: për [AllowAnonymous]
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ✅ SHTUAR: Auth endpoints zakonisht janë publike
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        // ✅ NDRYSHUAR: mos përdor ToLower() në DB query (më e pastër)
        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists) return BadRequest("Email already exists.");

        // ✅ SHTUAR: normalizim i rolit (ky është FIX për 403)
        var role = NormalizeRole(req.Role);

        var user = new User
        {
            FullName = req.FullName.Trim(),
            Email = email,
            Role = role
        };

        user.PasswordHash = _hasher.HashPassword(user, req.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { user.Id, user.FullName, user.Email, user.Role });
    }

    // ✅ SHTUAR: publike
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        // ✅ NDRYSHUAR: mos përdor ToLower() në DB query
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return Unauthorized("Invalid credentials.");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed) return Unauthorized("Invalid credentials.");

        var token = GenerateJwt(user);
        return Ok(new { access_token = token });
    }

    // ✅ SHTUAR: funksion për standardizimin e role-ve
    // Që JWT të ketë "Admin" ose "Employee", jo "admin"
    private static string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return "Employee";

        role = role.Trim();

        if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            return "Admin";

        if (string.Equals(role, "employee", StringComparison.OrdinalIgnoreCase))
            return "Employee";

        // nëse vjen rol tjetër i panjohur, e bëjmë Employee (safe default)
        return "Employee";
    }

    private string GenerateJwt(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"]!;
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;
        var expiresMinutes = int.Parse(jwt["ExpiresMinutes"] ?? "120");

        // Ensure we have the role name
        var role = NormalizeRole(user.Role);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, role) // ✅ NDRYSHUAR: tash del "Admin" e jo "admin"
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
