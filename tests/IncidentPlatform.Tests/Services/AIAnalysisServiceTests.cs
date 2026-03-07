using FluentAssertions;
using IncidentPlatform.Application.Services;
using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Tests.Services;

public class AIAnalysisServiceTests
{
    private readonly AIAnalysisService _service;

    public AIAnalysisServiceTests()
    {
        _service = new AIAnalysisService();
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_Critical_Database_When_Logs_Contain_Database_Keywords()
    {
        // Arrange
        var incident = new Incident { Id = Guid.NewGuid(), Title = "Checkout outage", Description = "Users cannot complete payment" };
        var logs = new List<LogEntry>
        {
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "checkout-api", LogLevel = "Critical", Message = "Primary database unavailable" }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("Critical");
        result.Category.Should().Be("database");
        result.RootCause.Should().Contain("Database layer failure");
        result.SuggestedFix.Should().Contain("connection pool");
        result.RecommendedTests.Should().HaveCount(3);
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_High_Network_When_Logs_Contain_Timeout()
    {
        // Arrange
        var incident = new Incident { Id = Guid.NewGuid(), Title = "Slow API", Description = "Requests are taking too long" };
        var logs = new List<LogEntry>
        {
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "orders-api", LogLevel = "Error", Message = "Timeout while calling downstream service" }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("High");
        result.Category.Should().Be("network");
        result.RootCause.Should().Contain("Network connectivity issue");
        result.SuggestedFix.Should().Contain("circuit-breaker");
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_Critical_Auth_When_Logs_Contain_Unauthorized()
    {
        // Arrange
        var incident = new Incident { Id = Guid.NewGuid(), Title = "Login failures", Description = "Users cannot authenticate" };
        var logs = new List<LogEntry>
        {
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "auth-api", LogLevel = "Error", Message = "Unauthorized access attempt detected" }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("Critical");
        result.Category.Should().Be("authentication");
        result.RootCause.Should().Contain("Authentication or authorization failure");
        result.SuggestedFix.Should().Contain("credentials");
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_Critical_Infrastructure_When_Logs_Contain_OOM()
    {
        // Arrange
        var incident = new Incident { Id = Guid.NewGuid(), Title = "Worker pod crash", Description = "Processing nodes are down" };
        var logs = new List<LogEntry>
        {
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "worker", LogLevel = "Critical", Message = "Out of memory: container killed by OOM killer" }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("Critical");
        result.Category.Should().Be("infrastructure");
        result.RootCause.Should().Contain("Infrastructure resource issue");
        result.SuggestedFix.Should().Contain("memory");
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_Medium_General_When_No_Keywords_Match()
    {
        // Arrange
        var incident = new Incident { Id = Guid.NewGuid(), Title = "Minor issue", Description = "Some users saw a warning" };
        var logs = new List<LogEntry>
        {
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "notification-api", LogLevel = "Warning", Message = "Unexpected formatting issue in email template" }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("Medium");
        result.Category.Should().Be("general");
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_Low_Unknown_When_Log_List_Is_Empty()
    {
        // Arrange
        var incident = new Incident { Id = Guid.NewGuid(), Title = "Unknown failure", Description = "No telemetry available" };
        var logs = new List<LogEntry>();

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("Low");
        result.Category.Should().Be("unknown");
        result.RootCause.Should().Be("No logs available to determine root cause.");
        result.SuggestedFix.Should().Contain("Capture more logs");
        result.RecommendedTests.Should().HaveCount(3);
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Use_Highest_Score_Category_When_Multiple_Keywords_Present()
    {
        // Arrange — database has 2 matches, network has 1
        var incident = new Incident { Id = Guid.NewGuid(), Title = "Mixed failure", Description = "DB and network issues" };
        var logs = new List<LogEntry>
        {
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "api", LogLevel = "Error", Message = "SQL deadlock detected on transaction" },
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "api", LogLevel = "Error", Message = "Connection pool exhausted" },
            new() { Id = Guid.NewGuid(), IncidentId = incident.Id, Service = "api", LogLevel = "Warning", Message = "Upstream timeout on payment gateway" }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Category.Should().Be("database");
    }
}
