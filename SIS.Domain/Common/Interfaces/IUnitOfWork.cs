using System;
using System.Threading.Tasks;

namespace SIS.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IStudentRepository Students { get; }
    ICourseRepository Courses { get; }
    IStudentCourseRepository StudentCourses { get; }
    Task<int> CompleteAsync(); // Saves all changes to DB
}