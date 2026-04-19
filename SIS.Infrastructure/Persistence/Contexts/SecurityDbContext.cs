using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIS.Domain;

namespace SIS.Infrastructure.Persistence.Contexts;

public class SecurityDbContext : IdentityDbContext<AppUser>
{
    public SecurityDbContext(DbContextOptions<SecurityDbContext> options)
        : base(options)
    {
    }
}