using FluentAssertions;
using Moq;
using IncidentPlatform.API.Controllers;
using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IncidentPlatform.Tests.Controllers;

public class AIControllerTests
{
    private readonly Mock<IIncidentService> _incidentServiceMock;
    private readonly Mock<ILogService> _logServiceMock;
    private readonly Mock<IAIAnalysisService> _aiServiceMock;
    private readonly AIController _controller;

    public AIControllerTests()
    {
        _incidentServiceMock = new Mock<IIncidentService>();
        _logServiceMock = new Mock<ILogService>();
        _aiServiceMock = new Mock<IAIAnalysisService>();
        _controller = new AIController(
            _incidentServiceMock.Object,
            _logServiceMock.Object,
            _aiServiceMock.Object);
    }

    [Fact]
    public async Task Analyze_Should_Return_NotFound_When_Incident_Does_Not_Exist()
    {
        // Arrange
        var incidentId = Guid.NewGuid();

        _incidentServiceMock
            .Setup(s => s.GetIncidentByIdAsync(incidentId))
            .ReturnsAsync((Incident?)null);

        // Act
        var result = await _controller.Analyze(incidentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();

        var notFound = result as NotFoundObjectResult;
        notFound!.Value.Should().Be($"Incident {incidentId} not found.");

        _logServiceMock.Verify(s => s.GetLogsByIncidentIdAsync(It.IsAny<Guid>()), Times.Never);
        _aiServiceMock.Verify(s => s.AnalyzeIncidentAsync(It.IsAny<Incident>(), It.IsAny<List<LogEntry>>()), Times.Never);
    }

    [Fact]
    public async Task Analyze_Should_Return_Ok_With_Analysis_When_Incident_Exists()
    {
        // Arrange
        var incidentId = Guid.NewGuid();

        var incident = new Incident
        {
            Id = incidentId,
            Title = "Production API timeout",
            Description = "Users report 504 errors during checkout"
        };

        var logs = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                Service = "checkout-api",
                LogLevel = "Critical",
                Message = "Primary database unavailable"
            }
        };

        var analysisResult = new AIAnalysisResult
        {
            Severity = "Critical",
            RootCause = "Possible issue detected from logs related to 'Production API timeout'.",
            SuggestedFix = "Review failing dependencies and inspect recent deployments.",
            RecommendedTests = new List<string>
            {
                "Add unit tests for the affected service method.",
                "Add integration tests for the failing workflow.",
                "Add regression tests covering the reported incident scenario."
            }
        };

        _incidentServiceMock
            .Setup(s => s.GetIncidentByIdAsync(incidentId))
            .ReturnsAsync(incident);

        _logServiceMock
            .Setup(s => s.GetLogsByIncidentIdAsync(incidentId))
            .ReturnsAsync(logs);

        _aiServiceMock
            .Setup(s => s.AnalyzeIncidentAsync(incident, logs))
            .ReturnsAsync(analysisResult);

        // Act
        var result = await _controller.Analyze(incidentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(analysisResult);

        _incidentServiceMock.Verify(s => s.GetIncidentByIdAsync(incidentId), Times.Once);
        _logServiceMock.Verify(s => s.GetLogsByIncidentIdAsync(incidentId), Times.Once);
        _aiServiceMock.Verify(s => s.AnalyzeIncidentAsync(incident, logs), Times.Once);
    }
}
