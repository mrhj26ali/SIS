using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using SIS.Domain;

namespace SIS.Infrastructure.Seeding;

public class RoleSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public RoleSeeder(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        string[] roles = { "Student", "Admin", "Staff" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        const string adminEmail = "admin@sis.local";
        const string adminPassword = "Admin@12345";

        var existingAdminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdminUser == null)
        {
            var adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            if (!await _userManager.IsInRoleAsync(existingAdminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(existingAdminUser, "Admin");
            }
        }
    }
}