using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIS.Domain;
using SIS.Domain.Common.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using SIS.Infrastructure.Repositories;
// ADD THESE USING STATEMENTS:
using FluentValidation;
using AutoMapper;

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
builder.Services.AddDbContext<SecurityDbContext>(options =>
    options.UseSqlServer(identityConnectionString, 
        x => x.MigrationsAssembly("SIS.Infrastructure")));

// 5. Configure Identity Services
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<SecurityDbContext>()
    .AddDefaultTokenProviders();

// 6. Register Business Logic Dependencies
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ===== FIX #1: AutoMapper Registration (Correct Syntax for v16.x) =====
builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<SIS.Application.Mappings.MappingProfile>();
});

// ===== FIX #2: FluentValidation Registration (Requires package installed above) =====
builder.Services.AddValidatorsFromAssemblyContaining<SIS.Application.Validators.Student.CreateStudentValidator>();

var app = builder.Build();

// 7. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();