using AutoMapper;
using Logging.Application.DTOs;
using Logging.Domain.Entities;

namespace Logging.Application.Profiles;

public class LoggingProfile : Profile
{
    public LoggingProfile()
    {
        CreateMap<CreateLogDto,Log>().ForMember(d => d.CreatedAt, opt => opt.Ignore())
        .ForMember(d => d.Id, opt => opt.Ignore());
    }
}