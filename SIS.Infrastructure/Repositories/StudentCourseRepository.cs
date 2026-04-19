using Microsoft.EntityFrameworkCore;
using SIS.Domain;
using SIS.Domain.Common.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using System.Threading.Tasks;

namespace SIS.Infrastructure.Repositories;

public class StudentCourseRepository : GenericRepository<StudentCourse>, IStudentCourseRepository
{
    public StudentCourseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> IsEnrolledAsync(int studentId, int courseId) =>
        await _context.StudentCourses.AnyAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

    public async Task<StudentCourse?> GetEnrollmentAsync(int studentId, int courseId) =>
        await _context.StudentCourses.FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

    public async Task RemoveEnrollmentAsync(int studentId, int courseId)
    {
        var enrollment = await GetEnrollmentAsync(studentId, courseId);
        if (enrollment != null)
            _context.StudentCourses.Remove(enrollment);
    }
}