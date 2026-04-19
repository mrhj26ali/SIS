using SIS.Domain;
using SIS.Domain.Common.Interfaces;
using SIS.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IStudentRepository Students { get; }
        public IGenericRepository<Course> Courses { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Students = new StudentRepository(_context);
            Courses = new GenericRepository<Course>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
