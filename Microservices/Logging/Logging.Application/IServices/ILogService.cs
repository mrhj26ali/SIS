using Logging.Application.DTOs;

namespace Logging.Application.IServices
{
    public interface ILogService
    {
        Task CreateLogAsync(CreateLogDto dto);
        Task<IEnumerable<LogDto>> GetAllAsync();
    }
}
