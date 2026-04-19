using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIS.Domain;
using SIS.Domain.Common.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using SIS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Get Connection Strings from appsettings.json
var businessConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var identityConnectionString = builder.Configuration.GetConnectionString("DefaultConnection0");

// 2. Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 3. Register the Business Database Context (ApplicationDbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(businessConnectionString));

// 4. Register the Identity Database Context (SecurityDbContext)
// We explicitly tell EF that migrations for this context live in SIS.Infrastructure
builder.Services.AddDbContext<SecurityDbContext>(options =>
    options.UseSqlServer(identityConnectionString, 
        x => x.MigrationsAssembly("SIS.Infrastructure")));

// 5. Configure Identity Services
// This uses the AppUser from SIS.Domain and the SecurityDbContext from SIS.Infrastructure
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        // Identity settings mirrored from your dummy project requirements
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        
        // Ensure unique emails for users
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<SecurityDbContext>()
    .AddDefaultTokenProviders();

// 6. Register Business Logic Dependencies
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// 7. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 8. Authentication & Authorization Middleware
// Authentication must come BEFORE Authorization
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();