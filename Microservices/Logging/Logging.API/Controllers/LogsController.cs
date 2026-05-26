using Logging.Application.DTOs;
using Logging.Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Logging.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpPost]
        public async Task<IActionResult> Log(CreateLogDto dto)
        {
            await _logService.CreateLogAsync(dto);
            return Ok("Logged");
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _logService.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var testResult = new CreateLogDto
            {
                Message = "Test message",
                CreatedBy = "Admin",
            };
            await _logService.CreateLogAsync(testResult);
            return Ok(testResult);
        }
    }

}
