
using Logging.Domain.Entities;

namespace Logging.Domain.IRepositories
{
    public interface ILogRepository
    {
        Task AddAsync(Log log);
        Task<IEnumerable<Log>> GetAllAsync();
    }

}
