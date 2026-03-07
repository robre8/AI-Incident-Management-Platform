using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;
using IncidentPlatform.Infrastructure.Data;
using MongoDB.Driver;

namespace IncidentPlatform.Infrastructure.Repositories;

public class LogRepository : ILogRepository
{
    private readonly MongoLogContext _context;

    public LogRepository(MongoLogContext context) => _context = context;

    public async Task AddAsync(LogEntry log)
    {
        await _context.Logs.InsertOneAsync(log);
    }

    public async Task<List<LogEntry>> GetByIncidentIdAsync(Guid incidentId)
    {
        return await _context.Logs
            .Find(x => x.IncidentId == incidentId)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }
}
