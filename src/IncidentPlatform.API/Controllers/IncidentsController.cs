using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IncidentPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentController : ControllerBase
{
    private readonly IIncidentService _service;

    public IncidentController(IIncidentService service)
    {
        _service = service;
    }

    // GET /api/incidents
    [HttpGet]
    public async Task<IEnumerable<Incident>> GetAll()
    {
        return await _service.GetAllIncidentsAsync();
    }

    // GET /api/incidents/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Incident>> GetById(Guid id)
    {
        var incident = await _service.GetIncidentByIdAsync(id);

        if (incident == null)
            return NotFound();

        return Ok(incident);
    }

    // POST /api/incidents
    [HttpPost]
    public async Task<ActionResult<Incident>> Create([FromBody] IncidentDTO dto)
    {
        var incident = await _service.CreateIncidentAsync(dto);

        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    // PUT /api/incidents/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] IncidentDTO dto)
    {
        var updated = await _service.UpdateIncidentAsync(id, dto);

        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    // DELETE /api/incidents/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteIncidentAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}