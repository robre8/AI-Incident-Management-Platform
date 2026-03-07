using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Interfaces;

public interface ILogService
{
    Task<LogEntry> CreateLogAsync(Guid incidentId, LogEntry log);
    Task<List<LogEntry>> GetLogsByIncidentIdAsync(Guid incidentId);
}
