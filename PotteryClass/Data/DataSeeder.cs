using Microsoft.AspNetCore.Identity;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!context.Users.Any(u => u.Role == UserRole.Admin))
        {
            var passwordHasher = new PasswordHasher<User>();

            var admin = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@pottery.local",
                Role = UserRole.Admin
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, "12345678");

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}