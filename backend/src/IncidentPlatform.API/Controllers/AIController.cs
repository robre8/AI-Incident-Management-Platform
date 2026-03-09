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
        try
        {
            var incident = await _incidentService.GetIncidentByIdAsync(incidentId);

            if (incident == null)
                return NotFound($"Incident {incidentId} not found.");

            List<Domain.Entities.LogEntry> logs = [];
            try
            {
                logs = await _logService.GetLogsByIncidentIdAsync(incidentId);
            }
            catch
            {
                // Keep AI analysis available even if Mongo logs are temporarily down.
                logs = [];
            }

            var result = await _aiService.AnalyzeIncidentAsync(incident, logs);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "AI analysis service unavailable"
            });
        }
    }
}
