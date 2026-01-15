using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;
using Microsoft.EntityFrameworkCore;

namespace Global_Strategist_Platform_Server.Data;

public static class DataSeeder
{
    public static async Task SeedAdminUserAsync(ApplicationDbContext context)
    {
        // Check if admin user already exists
        var adminExists = await context.Users
            .AnyAsync(u => u.Email == "admin@tbp.com" && !u.IsDeleted);

        if (adminExists)
        {
            return; // Admin already exists, skip seeding
        }

        // Create default admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "System Administrator",
            Email = "admin@tbp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), 
            Headline = "System Administrator",
            Country = "Global",
            ProfilePhotoUrl = string.Empty,
            IsActive = true,
            Role = Role.Admin,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }

    public static async Task SeedDefaultDataAsync(ApplicationDbContext context)
    {
        // Seed admin user
        await SeedAdminUserAsync(context);

        // Add other default data here if needed (categories, etc.)
    }
}

