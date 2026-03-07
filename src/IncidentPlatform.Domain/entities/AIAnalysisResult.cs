namespace IncidentPlatform.Domain.Entities;

public class AIAnalysisResult
{
    public string Severity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string SuggestedFix { get; set; } = string.Empty;
    public List<string> RecommendedTests { get; set; } = new();
}
