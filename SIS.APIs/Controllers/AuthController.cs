using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SIS.APIs.Requests;
using SIS.Application.DTOs.Student;
using SIS.Application.Interfaces;
using SIS.Contracts;
using SIS.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIS.APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly IStudentService _studentService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration config,
        IStudentService studentService,
        IPublishEndpoint publishEndpoint)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _studentService = studentService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            await _publishEndpoint.Publish(new LogMessage(
                $"Registration failed — email already exists: {model.Email}", "Auth"));
            return BadRequest(new { message = "Email already registered" });
        }

        if (await _studentService.StudentNumberExistsAsync(model.StudentNumber))
        {
            await _publishEndpoint.Publish(new LogMessage(
                $"Registration failed — student number already exists: {model.StudentNumber}", "Auth"));
            return Conflict(new { message = "Student number already exists." });
        }

        var user = new AppUser
        {
            UserName = string.IsNullOrWhiteSpace(model.UserName) ? model.Email : model.UserName,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            await _publishEndpoint.Publish(new LogMessage(
                $"Registration failed for {model.Email} — {errors}", "Auth"));
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "Student");

        var createStudentDto = new CreateStudentDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Age = model.Age,
            StudentNumber = model.StudentNumber,
            IdentityUserId = user.Id
        };

        try
        {
            await _studentService.CreateAsync(createStudentDto, user.Id);
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(user);
            await _publishEndpoint.Publish(new LogMessage(
                $"Registration rolled back for {model.Email} — {ex.Message}", "Auth"));

            if (ex.GetType().Name == "ConflictException")
                return Conflict(new { message = ex.Message });
            if (ex.GetType().Name.Contains("Validation"))
                return BadRequest(new { errors = ex.Message });
            return BadRequest(new { message = ex.Message });
        }

        await _publishEndpoint.Publish(new LogMessage(
            $"New student registered: {user.Email} (ID: {user.Id})", "Auth"));

        var token = await GenerateJwtToken(user);
        return Ok(new AuthResponse { Email = user.Email ?? string.Empty, Token = token });
    }

    [HttpPost("staff")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddStaff([FromBody] AddStaffRequest model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            await _publishEndpoint.Publish(new LogMessage(
                $"AddStaff failed — email already exists: {model.Email}", "Auth"));
            return BadRequest(new { message = "Email already registered" });
        }

        var user = new AppUser
        {
            UserName = string.IsNullOrWhiteSpace(model.UserName) ? model.Email : model.UserName,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            await _publishEndpoint.Publish(new LogMessage(
                $"AddStaff failed for {model.Email} — {errors}", "Auth"));
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "Staff");

        await _publishEndpoint.Publish(new LogMessage(
            $"New staff account created: {user.Email} (ID: {user.Id})", "Auth"));

        return Created(string.Empty, new { user.Id, user.Email });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            await _publishEndpoint.Publish(new LogMessage(
                $"Login failed — user not found: {model.Email}", "Auth"));
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            await _publishEndpoint.Publish(new LogMessage(
                $"Login failed — wrong password for: {model.Email}", "Auth"));
            return Unauthorized(new { message = "Invalid credentials" });
        }

        await _publishEndpoint.Publish(new LogMessage(
            $"User logged in: {user.Email} (ID: {user.Id})", "Auth"));

        var token = await GenerateJwtToken(user);
        return Ok(new AuthResponse { Email = user.Email ?? string.Empty, Token = token });
    }

    private async Task<string> GenerateJwtToken(AppUser user)
    {
        var email = user.Email ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty);
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var expiresInMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var expiry) ? expiry : 60;
        var expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}