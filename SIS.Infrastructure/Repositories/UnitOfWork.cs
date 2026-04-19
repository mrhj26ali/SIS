using SIS.Domain.Common.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using System;
using System.Threading.Tasks;

namespace SIS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed = false;

    public IStudentRepository Students { get; }
    public ICourseRepository Courses { get; }
    public IStudentCourseRepository StudentCourses { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Students = new StudentRepository(_context);
        Courses = new CourseRepository(_context);
        StudentCourses = new StudentCourseRepository(_context);
    }

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
            _context.Dispose();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}