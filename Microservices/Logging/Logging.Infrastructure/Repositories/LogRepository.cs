using Logging.Domain.Entities;
using Logging.Domain.IRepositories;
using Logging.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Logging.Infrastructure.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly LoggingDbContext _context;
        protected readonly DbSet<Log> _dbSet;

        public LogRepository(LoggingDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Log>();
        }

        public async Task AddAsync(Log log)
        {
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Log>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
    }

}
