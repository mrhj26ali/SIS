using SIS.Application.DTOs.Student;

namespace SIS.Application.Interfaces;

public interface IStudentService
{
    Task<IEnumerable<StudentListDto>> GetAllAsync();
    Task<StudentDetailDto> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateStudentDto dto);
    Task<bool> UpdateAsync(int id, UpdateStudentDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> EnrollInCourseAsync(int studentId, int courseId);
    Task<bool> UnenrollFromCourseAsync(int studentId, int courseId);
}