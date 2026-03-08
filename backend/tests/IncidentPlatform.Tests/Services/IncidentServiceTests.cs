using FluentAssertions;
using Moq;
using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Application.Services;
using IncidentPlatform.Domain.Entities;

namespace IncidentPlatform.Tests.Services;

public class IncidentServiceTests
{
    private readonly Mock<IIncidentRepository> _repoMock;
    private readonly IncidentService _service;

    public IncidentServiceTests()
    {
        _repoMock = new Mock<IIncidentRepository>();
        _service = new IncidentService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateIncidentAsync_Should_Create_Incident_With_New_Guid()
    {
        // Arrange
        var dto = new IncidentDTO
        {
            Title = "Production API slow",
            Description = "CPU usage above 90%"
        };

        Incident? savedIncident = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Incident>()))
            .Callback<Incident>(incident => savedIncident = incident)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateIncidentAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);

        savedIncident.Should().NotBeNull();
        savedIncident!.Id.Should().Be(result.Id);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Incident>()), Times.Once);
    }

    [Fact]
    public async Task GetAllIncidentsAsync_Should_Return_All_Incidents()
    {
        // Arrange
        var incidents = new List<Incident>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Server down",
                Description = "Main server not responding"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Login issue",
                Description = "Users cannot sign in"
            }
        };

        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(incidents);

        // Act
        var result = await _service.GetAllIncidentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(i => i.Title == "Server down");
    }

    [Fact]
    public async Task GetIncidentByIdAsync_Should_Return_Incident_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        var incident = new Incident
        {
            Id = id,
            Title = "Database timeout",
            Description = "Connection pool exhausted"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(incident);

        // Act
        var result = await _service.GetIncidentByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Title.Should().Be("Database timeout");
    }

    [Fact]
    public async Task UpdateIncidentAsync_Should_Update_Incident_When_It_Exists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingIncident = new Incident
        {
            Id = id,
            Title = "Old title",
            Description = "Old description"
        };

        var dto = new IncidentDTO
        {
            Title = "Updated title",
            Description = "Updated description"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingIncident);

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Incident>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateIncidentAsync(id, dto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);

        _repoMock.Verify(
            r => r.UpdateAsync(It.Is<Incident>(i =>
                i.Id == id &&
                i.Title == dto.Title &&
                i.Description == dto.Description)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateIncidentAsync_Should_Return_Null_When_Incident_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new IncidentDTO
        {
            Title = "Updated title",
            Description = "Updated description"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Incident?)null);

        // Act
        var result = await _service.UpdateIncidentAsync(id, dto);

        // Assert
        result.Should().BeNull();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Incident>()), Times.Never);
    }

    [Fact]
    public async Task DeleteIncidentAsync_Should_Return_True_When_Incident_Exists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var incident = new Incident
        {
            Id = id,
            Title = "Incident to delete",
            Description = "To be deleted"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(incident);

        _repoMock
            .Setup(r => r.DeleteAsync(incident))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteIncidentAsync(id);

        // Assert
        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(incident), Times.Once);
    }

    [Fact]
    public async Task DeleteIncidentAsync_Should_Return_False_When_Incident_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Incident?)null);

        // Act
        var result = await _service.DeleteIncidentAsync(id);

        // Assert
        result.Should().BeFalse();
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Incident>()), Times.Never);
    }
}
