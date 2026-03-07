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
        var logs = await _service.GetLogsByIncidentIdAsync(incidentId);
        return Ok(logs);
    }

    [HttpPost]
    public async Task<ActionResult<LogEntry>> Create(Guid incidentId, [FromBody] LogEntry log)
    {
        var created = await _service.CreateLogAsync(incidentId, log);
        return Ok(created);
    }
}
