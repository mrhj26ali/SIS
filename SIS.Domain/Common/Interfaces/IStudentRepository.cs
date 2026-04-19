using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Domain.Common.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        // A domain-specific requirement: find a student by their unique number
        Task<Student?> GetByStudentNumberAsync(string studentNumber);

        // Include courses for the dashboard view
        Task<Student?> GetStudentWithCoursesAsync(int id);
    }
}
