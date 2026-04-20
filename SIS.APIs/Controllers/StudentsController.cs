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

    [AllowAnonymous]
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
            var student = await _studentService.GetByIdAsync(id);
            return Ok(student);
        }
        catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous] // Allow public registration
    public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
    {
        try
        {
            var id = await _studentService.CreateAsync(dto);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
    {
        try
        {
            var result = await _studentService.UpdateAsync(id, dto);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _studentService.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex) when (ex.GetType().Name == "NotFoundException")
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{studentId}/courses/{courseId}")]
    public async Task<IActionResult> Enroll(int studentId, int courseId)
    {
        try
        {
            var result = await _studentService.EnrollInCourseAsync(studentId, courseId);
            return result ? Ok(new { message = "Enrolled successfully" }) : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{studentId}/courses/{courseId}")]
    public async Task<IActionResult> Unenroll(int studentId, int courseId)
    {
        try
        {
            var result = await _studentService.UnenrollFromCourseAsync(studentId, courseId);
            return result ? Ok(new { message = "Unenrolled successfully" }) : NotFound();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}