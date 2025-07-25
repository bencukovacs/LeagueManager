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

public class FixturesControllerTests
{
    private readonly Mock<IFixtureService> _mockFixtureService;
    private readonly FixturesController _controller;

    public FixturesControllerTests()
    {
        _mockFixtureService = new Mock<IFixtureService>();
        _controller = new FixturesController(_mockFixtureService.Object);
    }

    [Fact]
    public async Task GetFixtures_ReturnsOkResult_WithListOfFixtures()
    {
        // Arrange
        var fixtureDtoList = new List<FixtureResponseDto>();
        _mockFixtureService.Setup(s => s.GetAllFixturesAsync()).ReturnsAsync(fixtureDtoList);

        // Act
        var result = await _controller.GetFixtures();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<FixtureResponseDto>>(okResult.Value);
    }

    [Fact]
    public async Task GetFixture_WhenFixtureExists_ReturnsOkResult()
    {
        // Arrange
        var fixtureDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id=1, Name="A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id=2, Name="B", Status = "Approved"}, Status = "Scheduled"};
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);

        // Act
        var result = await _controller.GetFixture(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFixture = Assert.IsType<FixtureResponseDto>(okResult.Value);
        Assert.Equal(1, returnedFixture.Id);
    }

    [Fact]
    public async Task GetFixture_WhenFixtureDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(99)).ReturnsAsync((FixtureResponseDto?)null);

        // Act
        var result = await _controller.GetFixture(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateFixture_WhenSuccessful_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateFixtureDto { HomeTeamId = 1, AwayTeamId = 2 };
        var responseDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id=1, Name="A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id=2, Name="B", Status = "Approved" }, Status = "Scheduled" };
        _mockFixtureService.Setup(s => s.CreateFixtureAsync(createDto)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.CreateFixture(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetFixture", createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task UpdateFixture_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var updateDto = new UpdateFixtureDto();
        var responseDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id=1, Name="A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id=2, Name="B", Status = "Approved" }, Status = "Scheduled" };
        _mockFixtureService.Setup(s => s.UpdateFixtureAsync(1, updateDto)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.UpdateFixture(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task UpdateFixture_WhenFixtureNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateFixtureDto();
        _mockFixtureService.Setup(s => s.UpdateFixtureAsync(99, updateDto)).ReturnsAsync((FixtureResponseDto?)null);

        // Act
        var result = await _controller.UpdateFixture(99, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SubmitResult_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        var submitDto = new SubmitResultDto();
        var newResult = new Result { Id = 1, FixtureId = 1 };
        _mockFixtureService.Setup(s => s.SubmitResultAsync(1, submitDto)).ReturnsAsync(newResult);

        // Act
        var result = await _controller.SubmitResult(1, submitDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SubmitResult_WhenFixtureNotFound_ReturnsNotFound()
    {
        // Arrange
        var submitDto = new SubmitResultDto();
        _mockFixtureService.Setup(s => s.SubmitResultAsync(99, submitDto))
            .ThrowsAsync(new KeyNotFoundException("Fixture not found."));

        // Act
        var result = await _controller.SubmitResult(99, submitDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Fixture not found.", notFoundResult.Value);
    }
}