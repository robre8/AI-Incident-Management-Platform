using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Services;

public class LogService : ILogService
{
    private readonly ILogRepository _repository;

    public LogService(ILogRepository repository) => _repository = repository;

    public async Task<LogEntry> CreateLogAsync(Guid incidentId, LogEntry log)
    {
        log.Id = Guid.NewGuid();
        log.IncidentId = incidentId;
        log.Timestamp = DateTime.UtcNow;

        await _repository.AddAsync(log);
        return log;
    }

    public async Task<List<LogEntry>> GetLogsByIncidentIdAsync(Guid incidentId)
    {
        return await _repository.GetByIncidentIdAsync(incidentId);
    }
}
