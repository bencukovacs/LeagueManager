using Xunit;
using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LeagueManager.Tests.Controllers;

public class LocationsControllerTests
{
    private readonly Mock<ILocationService> _mockLocationService;
    private readonly LocationsController _controller;

    public LocationsControllerTests()
    {
        _mockLocationService = new Mock<ILocationService>();
        _controller = new LocationsController(_mockLocationService.Object);
    }

    [Fact]
    public async Task GetLocations_ReturnsOkResult_WithListOfLocations()
    {
        // Arrange
        var locations = new List<LocationResponseDto> { new() { Id = 1, Name = "Pitch 1" } };
        _mockLocationService.Setup(s => s.GetAllLocationsAsync()).ReturnsAsync(locations);

        // Act
        var result = await _controller.GetLocations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLocations = Assert.IsAssignableFrom<IEnumerable<LocationResponseDto>>(okResult.Value);
        Assert.Single(returnedLocations);
    }

    [Fact]
    public async Task GetLocation_WhenLocationExists_ReturnsOkResult()
    {
        // Arrange
        var location = new LocationResponseDto { Id = 1, Name = "Main Pitch" };
        _mockLocationService.Setup(s => s.GetLocationByIdAsync(1)).ReturnsAsync(location);

        // Act
        var result = await _controller.GetLocation(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<LocationResponseDto>(okResult.Value);
    }

    [Fact]
    public async Task CreateLocation_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new LocationDto { Name = "Training Ground" };
        var newLocation = new LocationResponseDto { Id = 1, Name = "Training Ground" };
        _mockLocationService.Setup(s => s.CreateLocationAsync(dto)).ReturnsAsync(newLocation);

        // Act
        var result = await _controller.CreateLocation(dto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetLocation", createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.RouteValues);
        Assert.Equal(1, createdAtActionResult.RouteValues["id"]);
    }

    [Fact]
    public async Task UpdateLocation_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var dto = new LocationDto { Name = "Updated Name" };
        _mockLocationService.Setup(s => s.UpdateLocationAsync(1, dto))
                            .ReturnsAsync(new LocationResponseDto { Id = 1, Name = "Updated Name" });

        // Act
        var result = await _controller.UpdateLocation(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateLocation_WhenLocationNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new LocationDto { Name = "Updated Name" };
        _mockLocationService.Setup(s => s.UpdateLocationAsync(99, dto)).ReturnsAsync((LocationResponseDto?)null);

        // Act
        var result = await _controller.UpdateLocation(99, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteLocation_WhenLocationIsUnused_ReturnsNoContent()
    {
        // Arrange
        _mockLocationService.Setup(s => s.DeleteLocationAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteLocation(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteLocation_WhenLocationIsInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        var expectedExceptionMessage = "Cannot delete a location that is currently assigned to a fixture.";
        _mockLocationService.Setup(service => service.DeleteLocationAsync(1))
            .ThrowsAsync(new InvalidOperationException(expectedExceptionMessage));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteLocation(1));
        
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }
}