using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IncidentPlatform.API.Controllers;

[ApiController]
[Route("api/incidents/{incidentId:guid}/logs")]
public class LogController : ControllerBase
{
    private readonly ILogService _service;

    public LogController(ILogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LogEntry>>> GetByIncidentId(Guid incidentId)
    {
        try
        {
            var logs = await _service.GetLogsByIncidentIdAsync(incidentId);
            return Ok(logs);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Logs service unavailable"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<LogEntry>> Create(Guid incidentId, [FromBody] LogEntry log)
    {
        try
        {
            var created = await _service.CreateLogAsync(incidentId, log);
            return Ok(created);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Logs service unavailable"
            });
        }
    }
}
