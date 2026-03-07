using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Interfaces;

public interface IAIAnalysisService
{
    Task<AIAnalysisResult> AnalyzeIncidentAsync(Incident incident, List<LogEntry> logs);
}
