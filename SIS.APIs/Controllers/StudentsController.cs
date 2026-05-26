using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Application.DTOs.Student;
using SIS.Application.Interfaces;
namespace SIS.APIs.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _studentService.GetAllAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            if (User.IsInRole("Student") && await IsStudentIdMismatch(id))
                return Forbid();

            var student = await _studentService.GetByIdAsync(id);
            return Ok(student);
        }
        catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
    {
        try
        {
            var id = await _studentService.CreateAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        catch (Exception ex)
        {
            if (ex.GetType().Name.Contains("Validation"))
                return BadRequest(new { errors = ex.Message });
            if (ex.GetType().Name == "ConflictException")
                return Conflict(new { message = ex.Message });
            throw;
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
    {
        try
        {
            var result = await _studentService.UpdateAsync(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            var result = await _studentService.DeleteAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Staff,Student")]
    [HttpPost("{studentId}/courses/{courseId}")]
    public async Task<IActionResult> Enroll(int studentId, int courseId)
    {
        try
        {
            if (User.IsInRole("Student") && await IsStudentIdMismatch(studentId))
                return Forbid();

            var result = await _studentService.EnrollInCourseAsync(studentId, courseId, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return result ? Ok(new { message = "Enrolled successfully" }) : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Staff,Student")]
    [HttpDelete("{studentId}/courses/{courseId}")]
    public async Task<IActionResult> Unenroll(int studentId, int courseId)
    {
        try
        {
            if (User.IsInRole("Student") && await IsStudentIdMismatch(studentId))
                return Forbid();

            var result = await _studentService.UnenrollFromCourseAsync(studentId, courseId, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return result ? Ok(new { message = "Unenrolled successfully" }) : NotFound();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private async Task<bool> IsStudentIdMismatch(int studentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return true;

        try
        {
            var currentStudent = await _studentService.GetByIdentityUserIdAsync(userId);
            return currentStudent.Id != studentId;
        }
        catch
        {
            return true;
        }
    }
}