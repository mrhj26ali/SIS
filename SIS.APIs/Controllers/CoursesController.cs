using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Application.DTOs.Course;
using SIS.Application.Interfaces;
namespace SIS.APIs.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var course = await _courseService.GetByIdAsync(id);
            return Ok(course);
        }
        catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
    {
        try
        {
            var id = await _courseService.CreateAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Validation"))
        {
            return BadRequest(new { errors = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
    {
        try
        {
            var result = await _courseService.UpdateAsync(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Validation"))
        {
            return BadRequest(new { errors = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _courseService.DeleteAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}