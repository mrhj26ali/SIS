using Microsoft.AspNetCore.Identity;

namespace SIS.Domain;

public class AppUser : IdentityUser
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}