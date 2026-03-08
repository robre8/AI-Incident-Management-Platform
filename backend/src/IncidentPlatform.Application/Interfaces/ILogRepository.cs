using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Interfaces;

public interface ILogRepository
{
    Task AddAsync(LogEntry log);
    Task<List<LogEntry>> GetByIncidentIdAsync(Guid incidentId);
}
