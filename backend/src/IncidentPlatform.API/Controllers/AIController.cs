using IncidentPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IncidentPlatform.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly IIncidentService _incidentService;
    private readonly ILogService _logService;
    private readonly IAIAnalysisService _aiService;

    public AIController(
        IIncidentService incidentService,
        ILogService logService,
        IAIAnalysisService aiService)
    {
        _incidentService = incidentService;
        _logService = logService;
        _aiService = aiService;
    }

    [HttpPost("analyze/{incidentId:guid}")]
    public async Task<IActionResult> Analyze(Guid incidentId)
    {
        var incident = await _incidentService.GetIncidentByIdAsync(incidentId);

        if (incident == null)
            return NotFound($"Incident {incidentId} not found.");

        var logs = await _logService.GetLogsByIncidentIdAsync(incidentId);
        var result = await _aiService.AnalyzeIncidentAsync(incident, logs);

        return Ok(result);
    }
}
