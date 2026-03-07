using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Services;

public class AIAnalysisService : IAIAnalysisService
{
    public Task<AIAnalysisResult> AnalyzeIncidentAsync(Incident incident, List<LogEntry> logs)
    {
        var joinedLogs = string.Join(" ", logs.Select(l => l.Message).Take(10));

        var severity = "Medium";

        if (joinedLogs.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
            joinedLogs.Contains("unavailable", StringComparison.OrdinalIgnoreCase) ||
            joinedLogs.Contains("crash", StringComparison.OrdinalIgnoreCase))
        {
            severity = "High";
        }

        if (joinedLogs.Contains("database", StringComparison.OrdinalIgnoreCase))
        {
            severity = "Critical";
        }

        var result = new AIAnalysisResult
        {
            Severity = severity,
            RootCause = logs.Count == 0
                ? "No logs available to determine root cause."
                : $"Possible issue detected from logs related to '{incident.Title}'.",
            SuggestedFix = logs.Count == 0
                ? "Capture more logs and retry analysis."
                : "Review failing dependencies, inspect recent deployments, and increase automated test coverage around the affected flow.",
            RecommendedTests = new List<string>
            {
                "Add unit tests for the affected service method.",
                "Add integration tests for the failing workflow.",
                "Add regression tests covering the reported incident scenario."
            }
        };

        return Task.FromResult(result);
    }
}
