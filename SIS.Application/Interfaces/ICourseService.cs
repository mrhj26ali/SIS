using SIS.Application.DTOs.Course;

namespace SIS.Application.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseListDto>> GetAllAsync();
    Task<CourseListDto> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateCourseDto dto, string? identityUserId = null);
    Task<bool> UpdateAsync(int id, UpdateCourseDto dto, string? identityUserId = null);
    Task<bool> DeleteAsync(int id, string? identityUserId = null);
}