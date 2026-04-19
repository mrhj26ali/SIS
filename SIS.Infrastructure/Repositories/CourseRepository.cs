using Microsoft.EntityFrameworkCore;
using SIS.Domain;
using SIS.Domain.Common.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIS.Infrastructure.Repositories;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Course?> GetCourseWithEnrollmentsAsync(int id) =>
        await _context.Courses
            .Include(c => c.StudentCourses)
            .ThenInclude(sc => sc.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Course>> GetCoursesByStudentIdAsync(int studentId) =>
        await _context.Courses
            .Include(c => c.StudentCourses)
            .Where(c => c.StudentCourses.Any(sc => sc.StudentId == studentId))
            .ToListAsync();

    public async Task<int> GetEnrollmentCountAsync(int courseId) =>
        await _context.StudentCourses.CountAsync(sc => sc.CourseId == courseId);
}