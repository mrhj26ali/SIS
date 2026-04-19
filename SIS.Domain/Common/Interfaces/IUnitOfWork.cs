using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Domain.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IGenericRepository<Course> Courses { get; }
        Task<int> CompleteAsync(); // Saves changes to the DB
    }
}
