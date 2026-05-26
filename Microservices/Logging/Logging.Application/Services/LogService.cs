using AutoMapper;
using Logging.Application.DTOs;
using Logging.Domain.Entities;
using Logging.Domain.IRepositories;
using Logging.Application.IServices;

namespace Logging.Application.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;
        private readonly IMapper _mapper;

        public LogService(ILogRepository logRepository, IMapper mapper)
        {
            _logRepository = logRepository;
            _mapper = mapper;
        }

        public async Task CreateLogAsync(CreateLogDto dto)
        {
            var log = _mapper.Map<Log>(dto);

            await _logRepository.AddAsync(log);
        }

        public async Task<IEnumerable<LogDto>> GetAllAsync()
        {
            var logs = await _logRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<LogDto>>(logs);
        }

    }

}
