using FluentAssertions;
using Moq;
using IncidentPlatform.API.Controllers;
using IncidentPlatform.Application.Interfaces;
using IncidentPlatform.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IncidentPlatform.Tests.Controllers;

public class IncidentControllerTests
{
    private readonly Mock<IIncidentService> _serviceMock;
    private readonly IncidentController _controller;

    public IncidentControllerTests()
    {
        _serviceMock = new Mock<IIncidentService>();
        _controller = new IncidentController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Service_Returns_Null()
    {
        // Arrange
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.GetIncidentByIdAsync(id))
            .ReturnsAsync((Incident?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _serviceMock.Verify(s => s.GetIncidentByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task Create_Should_Return_CreatedAtAction_With_Created_Incident()
    {
        // Arrange
        var dto = new IncidentDTO
        {
            Title = "API outage",
            Description = "Service unavailable"
        };

        var createdIncident = new Incident
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description
        };

        _serviceMock
            .Setup(s => s.CreateIncidentAsync(dto))
            .ReturnsAsync(createdIncident);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var createdAt = result.Result as CreatedAtActionResult;
        createdAt.Should().NotBeNull();
        createdAt!.ActionName.Should().Be(nameof(IncidentController.GetById));
        createdAt.RouteValues.Should().ContainKey("id");
        createdAt.RouteValues!["id"].Should().Be(createdIncident.Id);
        createdAt.Value.Should().BeEquivalentTo(createdIncident);

        _serviceMock.Verify(s => s.CreateIncidentAsync(dto), Times.Once);
    }
}
