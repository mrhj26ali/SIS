using System.Threading.Tasks;

namespace SIS.Domain.Common.Interfaces;

public interface IStudentCourseRepository : IGenericRepository<StudentCourse>
{
    Task<bool> IsEnrolledAsync(int studentId, int courseId);
    Task<StudentCourse?> GetEnrollmentAsync(int studentId, int courseId);
    Task RemoveEnrollmentAsync(int studentId, int courseId);
}