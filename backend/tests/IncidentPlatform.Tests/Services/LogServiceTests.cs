using FluentAssertions;
using Moq;
using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Application.Services;
using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Tests.Services;

public class LogServiceTests
{
    private readonly Mock<ILogRepository> _repoMock;
    private readonly LogService _service;

    public LogServiceTests()
    {
        _repoMock = new Mock<ILogRepository>();
        _service = new LogService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateLogAsync_Should_Assign_New_Guid_IncidentId_And_UtcTimestamp()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var log = new LogEntry
        {
            Service = "checkout-api",
            LogLevel = "Error",
            Message = "Database timeout while processing payment"
        };

        LogEntry? savedLog = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<LogEntry>()))
            .Callback<LogEntry>(l => savedLog = l)
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;

        // Act
        var result = await _service.CreateLogAsync(incidentId, log);

        var after = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.IncidentId.Should().Be(incidentId);
        result.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        result.Message.Should().Be("Database timeout while processing payment");

        savedLog.Should().NotBeNull();
        savedLog!.Id.Should().Be(result.Id);
        savedLog.IncidentId.Should().Be(incidentId);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<LogEntry>()), Times.Once);
    }

    [Fact]
    public async Task GetLogsByIncidentIdAsync_Should_Return_Logs_For_Incident()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var logs = new List<LogEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                Service = "checkout-api",
                LogLevel = "Critical",
                Message = "Primary database unavailable",
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                Service = "checkout-api",
                LogLevel = "Error",
                Message = "Database timeout while processing payment",
                Timestamp = DateTime.UtcNow.AddSeconds(-5)
            }
        };

        _repoMock
            .Setup(r => r.GetByIncidentIdAsync(incidentId))
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetLogsByIncidentIdAsync(incidentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.IncidentId.Should().Be(incidentId));
        result.Should().Contain(l => l.LogLevel == "Critical");
        result.Should().Contain(l => l.LogLevel == "Error");

        _repoMock.Verify(r => r.GetByIncidentIdAsync(incidentId), Times.Once);
    }

    [Fact]
    public async Task GetLogsByIncidentIdAsync_Should_Return_Empty_List_When_No_Logs_Exist()
    {
        // Arrange
        var incidentId = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIncidentIdAsync(incidentId))
            .ReturnsAsync(new List<LogEntry>());

        // Act
        var result = await _service.GetLogsByIncidentIdAsync(incidentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _repoMock.Verify(r => r.GetByIncidentIdAsync(incidentId), Times.Once);
    }

    [Fact]
    public async Task CreateLogAsync_Should_Preserve_Service_LogLevel_And_Message()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var log = new LogEntry
        {
            Service = "payments-api",
            LogLevel = "Warning",
            Message = "Retry attempt 3 of 5 for payment processor"
        };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<LogEntry>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateLogAsync(incidentId, log);

        // Assert
        result.Service.Should().Be("payments-api");
        result.LogLevel.Should().Be("Warning");
        result.Message.Should().Be("Retry attempt 3 of 5 for payment processor");
    }
}
