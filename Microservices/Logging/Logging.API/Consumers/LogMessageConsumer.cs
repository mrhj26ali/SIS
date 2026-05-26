using MassTransit;
using SIS.Contracts;
using Logging.Application.DTOs;
using Logging.Application.IServices;

namespace Logging.API.Consumers;

public class LogMessageConsumer : IConsumer<LogMessage>
{
    private readonly ILogService _logService;
    public LogMessageConsumer(ILogService logService) => _logService = logService;

    public async Task Consume(ConsumeContext<LogMessage> context)
    {
        var dto = new CreateLogDto
        {
            Message = context.Message.Message,
            CreatedBy = context.Message.CreatedBy
        };
        await _logService.CreateLogAsync(dto);
    }
}