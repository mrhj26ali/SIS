using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace SIS.Infrastructure.Seeding;

public class RoleSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        // Define roles required by your system
        string[] roles = { "Student", "Admin", "Staff" };

        foreach (var roleName in roles)
        {
            // Only create if it doesn't already exist (idempotent)
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}