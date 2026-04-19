using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIS.Domain.Common.Interfaces;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<Course?> GetCourseWithEnrollmentsAsync(int id);
    Task<IEnumerable<Course>> GetCoursesByStudentIdAsync(int studentId);
    Task<int> GetEnrollmentCountAsync(int courseId);
}