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
    public async Task AnalyzeIncidentAsync_Should_Return_Critical_When_Logs_Contain_Database()
    {
        // Arrange
        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Title = "Checkout outage",
            Description = "Users cannot complete payment"
        };

        var logs = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                Service = "checkout-api",
                LogLevel = "Critical",
                Message = "Primary database unavailable"
            }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Should().NotBeNull();
        result.Severity.Should().Be("Critical");
        result.RootCause.Should().NotBeNullOrWhiteSpace();
        result.SuggestedFix.Should().NotBeNullOrWhiteSpace();
        result.RecommendedTests.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_High_When_Logs_Contain_Timeout()
    {
        // Arrange
        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Title = "Slow API",
            Description = "Requests are taking too long"
        };

        var logs = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                Service = "orders-api",
                LogLevel = "Error",
                Message = "Timeout while calling downstream service"
            }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("High");
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_Medium_When_No_Important_Keywords_Are_Found()
    {
        // Arrange
        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Title = "Minor issue",
            Description = "Some users saw a warning"
        };

        var logs = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                Service = "notification-api",
                LogLevel = "Warning",
                Message = "Unexpected formatting issue in email template"
            }
        };

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("Medium");
    }

    [Fact]
    public async Task AnalyzeIncidentAsync_Should_Return_No_Logs_Message_When_Log_List_Is_Empty()
    {
        // Arrange
        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Title = "Unknown failure",
            Description = "No telemetry available"
        };

        var logs = new List<LogEntry>();

        // Act
        var result = await _service.AnalyzeIncidentAsync(incident, logs);

        // Assert
        result.Severity.Should().Be("Medium");
        result.RootCause.Should().Be("No logs available to determine root cause.");
        result.SuggestedFix.Should().Be("Capture more logs and retry analysis.");
        result.RecommendedTests.Should().HaveCount(3);
    }
}
