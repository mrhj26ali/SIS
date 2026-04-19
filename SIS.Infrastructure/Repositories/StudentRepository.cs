using Microsoft.EntityFrameworkCore;
using SIS.Domain;
using SIS.Domain.Common.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Infrastructure.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Student?> GetByStudentNumberAsync(string studentNumber)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<Student?> GetStudentWithCoursesAsync(int id)
        {
            return await _context.Students
                .Include(s => s.StudentCourses)
                .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
