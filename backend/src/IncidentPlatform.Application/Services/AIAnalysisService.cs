using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Application.Services;

public class AIAnalysisService : IAIAnalysisService
{
    private static readonly IReadOnlyDictionary<string, string[]> CategoryKeywords =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["database"]       = ["database", "sql", "deadlock", "connection pool", "query timeout", "transaction", "db error", "mongo", "replica"],
            ["infrastructure"] = ["cpu", "memory", "disk", "out of memory", "oom", "pod", "node", "container", "crash", "restart", "health check", "kubernetes", "deploy"],
            ["authentication"] = ["unauthorized", "forbidden", "401", "403", "jwt", "token", "oauth", "invalid credentials", "permission denied", "auth"],
            ["network"]        = ["timeout", "connection refused", "unavailable", "dns", "502", "503", "504", "network", "upstream", "latency", "packet loss"],
        };

    private static readonly IReadOnlyDictionary<string, string[]> CriticalKeywords =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["database"]       = ["database", "deadlock", "connection pool", "db error", "mongo", "replica"],
            ["infrastructure"] = ["out of memory", "oom", "crash"],
            ["authentication"] = ["unauthorized", "403", "permission denied"],
            ["network"]        = ["connection refused", "unavailable"],
        };

    private static readonly IReadOnlyDictionary<string, string[]> HighKeywords =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["database"]       = ["query timeout", "transaction", "sql"],
            ["infrastructure"] = ["cpu", "memory", "restart", "pod", "node", "health check"],
            ["authentication"] = ["401", "jwt", "token", "oauth", "invalid credentials"],
            ["network"]        = ["timeout", "502", "503", "504", "latency"],
        };

    public Task<AIAnalysisResult> AnalyzeIncidentAsync(Incident incident, List<LogEntry> logs)
    {
        if (logs.Count == 0)
        {
            return Task.FromResult(new AIAnalysisResult
            {
                Severity = "Low",
                Category = "unknown",
                RootCause = "No logs available to determine root cause.",
                SuggestedFix = "Capture more logs and retry analysis. Enable verbose logging on the affected service.",
                RecommendedTests =
                [
                    "Add structured logging to all service entry points.",
                    "Verify log collection pipeline is operational.",
                    "Add a smoke test to confirm the service is reachable."
                ]
            });
        }

        var joinedLogs = string.Join(" ", logs.Select(l => l.Message).Take(20));

        var category = DetectCategory(joinedLogs);
        var severity = DetectSeverity(joinedLogs, category);
        var (rootCause, suggestedFix, recommendedTests) = BuildDiagnostics(incident, category, severity);

        return Task.FromResult(new AIAnalysisResult
        {
            Severity = severity,
            Category = category,
            RootCause = rootCause,
            SuggestedFix = suggestedFix,
            RecommendedTests = recommendedTests
        });
    }

    private static string DetectCategory(string joinedLogs)
    {
        // Score each category by how many of its keywords appear in the logs
        var scores = CategoryKeywords.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Count(kw => joinedLogs.Contains(kw, StringComparison.OrdinalIgnoreCase)));

        var best = scores.MaxBy(kv => kv.Value);
        return best.Value > 0 ? best.Key : "general";
    }

    private static string DetectSeverity(string joinedLogs, string category)
    {
        if (category != "general" && CriticalKeywords.TryGetValue(category, out var critWords))
        {
            if (critWords.Any(kw => joinedLogs.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                return "Critical";
        }

        if (category != "general" && HighKeywords.TryGetValue(category, out var highWords))
        {
            if (highWords.Any(kw => joinedLogs.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                return "High";
        }

        return "Medium";
    }

    private static (string rootCause, string suggestedFix, List<string> recommendedTests)
        BuildDiagnostics(Incident incident, string category, string severity)
    {
        var rootCause = category switch
        {
            "database"       => $"Database layer failure detected in incident '{incident.Title}'. Potential causes: connection exhaustion, query deadlock, or replica lag.",
            "infrastructure" => $"Infrastructure resource issue detected in incident '{incident.Title}'. Potential causes: memory pressure, pod crash-loop, or failed health checks.",
            "authentication" => $"Authentication or authorization failure detected in incident '{incident.Title}'. Potential causes: expired tokens, misconfigured permissions, or credential rotation.",
            "network"        => $"Network connectivity issue detected in incident '{incident.Title}'. Potential causes: upstream service unavailability, DNS failure, or elevated latency.",
            _                => $"Unclassified issue detected in incident '{incident.Title}'. Review logs and service dependencies."
        };

        var suggestedFix = (category, severity) switch
        {
            ("database", "Critical")       => "Immediately check database connection pool saturation and active locks. Restart the connection pool if necessary. Review slow query logs and consider read-replica failover.",
            ("database", "High")           => "Inspect long-running queries and transaction locks. Tune query indexes and review ORM N+1 patterns. Increase connection pool limits if under load.",
            ("database", _)                => "Monitor database query performance metrics. Review recent schema migrations and index usage.",
            ("infrastructure", "Critical") => "Immediately check pod/container memory usage. Scale up the affected nodes or restart the failing containers. Review OOM killer logs.",
            ("infrastructure", "High")     => "Review resource quotas and pod autoscaling policies. Inspect recent deployment changes and rollback if correlated. Check readiness/liveness probe configuration.",
            ("infrastructure", _)          => "Review infrastructure health dashboards. Verify deployment manifests and resource limits are correctly configured.",
            ("authentication", "Critical") => "Investigate unauthorized access patterns immediately. Rotate affected credentials and tokens. Audit permission grants in the last 24 hours.",
            ("authentication", "High")     => "Review token expiration policies and refresh flows. Ensure OAuth scopes are correctly configured. Check for recent changes to role or permission assignments.",
            ("authentication", _)          => "Verify API key and token configurations. Review auth middleware and ensure consistent header propagation across services.",
            ("network", "Critical")        => "Check upstream service availability and DNS resolution. Inspect load balancer health checks. Review firewall rules and network security group policies.",
            ("network", "High")            => "Inspect service mesh configuration and retry policies. Review timeout thresholds and circuit-breaker settings. Check for elevated error rates on dependent services.",
            ("network", _)                 => "Review network latency metrics between services. Verify timeout configurations align with SLA requirements.",
            _                              => "Review service logs and recent deployments. Inspect dependencies and escalate to the responsible team if unresolved."
        };

        List<string> recommendedTests = category switch
        {
            "database" => new List<string>
            {
                "Add integration tests simulating connection pool exhaustion.",
                "Add unit tests for repository retry and timeout handling.",
                "Add chaos tests that inject database unavailability and verify graceful degradation."
            },
            "infrastructure" => new List<string>
            {
                "Add load tests to validate autoscaling triggers at expected thresholds.",
                "Add unit tests for graceful shutdown and resource cleanup handlers.",
                "Add health check endpoint tests verifying correct status codes under resource pressure."
            },
            "authentication" => new List<string>
            {
                "Add unit tests for token validation covering expired, malformed, and revoked tokens.",
                "Add integration tests for each protected endpoint verifying 401/403 responses.",
                "Add regression tests for permission boundary scenarios."
            },
            "network" => new List<string>
            {
                "Add integration tests with simulated upstream timeouts validating circuit-breaker behavior.",
                "Add unit tests for retry policies and backoff strategies.",
                "Add end-to-end tests covering the full request path under degraded network conditions."
            },
            _ => new List<string>
            {
                "Add unit tests for the affected service methods.",
                "Add integration tests for the failing workflow.",
                "Add regression tests covering the reported incident scenario."
            }
        };

        return (rootCause, suggestedFix, recommendedTests);
    }
}
