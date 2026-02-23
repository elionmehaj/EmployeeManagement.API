using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Kontrollojmë nëse ka përdorues në tabelë
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var hasher = new PasswordHasher<User>();

        var mockUsers = new List<User>
        {
            new User 
            { 
                FullName = "Admin Principal", 
                Email = "admin@pulse.com", 
                Role = "Admin", 
                Department = Department.IT 
            },
            new User 
            { 
                FullName = "Arben Krasniqi", 
                Email = "arben.k@pulse.com", 
                Role = "Employee", 
                Department = Department.Engineering 
            },
            new User 
            { 
                FullName = "Blerina Hoxha", 
                Email = "blerina.h@pulse.com", 
                Role = "Employee", 
                Department = Department.HR 
            },
            new User 
            { 
                FullName = "Driton Gashi", 
                Email = "driton.g@pulse.com", 
                Role = "Employee", 
                Department = Department.Marketing 
            },
            new User 
            { 
                FullName = "Emina Kelmendi", 
                Email = "emina.k@pulse.com", 
                Role = "Employee", 
                Department = Department.Finance 
            },
            new User 
            { 
                FullName = "Faton Berisha", 
                Email = "faton.b@pulse.com", 
                Role = "Employee", 
                Department = Department.Sales 
            },
            new User 
            { 
                FullName = "Gresa Morina", 
                Email = "gresa.m@pulse.com", 
                Role = "Employee", 
                Department = Department.IT 
            },
            new User 
            { 
                FullName = "Hekuran Shala", 
                Email = "hekuran.s@pulse.com", 
                Role = "Employee", 
                Department = Department.CustomerSupport 
            },
            new User 
            { 
                FullName = "Iliriana Leka", 
                Email = "iliriana.l@pulse.com", 
                Role = "Employee", 
                Department = Department.Engineering 
            },
            new User 
            { 
                FullName = "Luan Rama", 
                Email = "luan.r@pulse.com", 
                Role = "Employee", 
                Department = Department.Engineering 
            }
        };

        foreach (var user in mockUsers)
        {
            user.PasswordHash = hasher.HashPassword(user, "DefaultPassword123!");
        }

        await context.Users.AddRangeAsync(mockUsers);
        await context.SaveChangesAsync();
    }
}
