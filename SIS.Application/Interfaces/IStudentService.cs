using SIS.Application.DTOs.Student;

namespace SIS.Application.Interfaces;

public interface IStudentService
{
    Task<IEnumerable<StudentListDto>> GetAllAsync();
    Task<StudentDetailDto> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateStudentDto dto, string? identityUserId = null);
    Task<bool> StudentNumberExistsAsync(string studentNumber);
    Task<StudentDetailDto> GetByIdentityUserIdAsync(string identityUserId);
    Task<bool> UpdateAsync(int id, UpdateStudentDto dto, string? identityUserId = null);
    Task<bool> DeleteAsync(int id, string? identityUserId = null);
    Task<bool> EnrollInCourseAsync(int studentId, int courseId, string? identityUserId = null);
    Task<bool> UnenrollFromCourseAsync(int studentId, int courseId, string? identityUserId = null);
}