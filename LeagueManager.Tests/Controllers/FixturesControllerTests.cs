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
    public async Task GetFixture_WhenFixtureExists_ReturnsOkResult()
    {
        // Arrange
        var fixture = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixture);

        // Act
        var result = await _controller.GetFixture(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFixture = Assert.IsType<Fixture>(okResult.Value);
        Assert.Equal(1, returnedFixture.Id);
    }

    [Fact]
    public async Task GetFixture_WhenFixtureDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(99)).ReturnsAsync((Fixture?)null);

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
        var newFixture = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        var createdFixtureWithIncludes = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2, HomeTeam = new Team { Name = "A" }, AwayTeam = new Team { Name = "B" } };

        _mockFixtureService.Setup(s => s.CreateFixtureAsync(createDto)).ReturnsAsync(newFixture);
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(newFixture.Id)).ReturnsAsync(createdFixtureWithIncludes);


        // Act
        var result = await _controller.CreateFixture(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetFixture", createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task CreateFixture_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateFixtureDto();
        _mockFixtureService.Setup(s => s.CreateFixtureAsync(createDto))
            .ThrowsAsync(new ArgumentException("Some error"));

        // Act
        var result = await _controller.CreateFixture(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Some error", badRequestResult.Value);
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
    
    [Fact]
    public async Task SubmitResult_WhenResultExists_ReturnsBadRequest()
    {
        // Arrange
        var submitDto = new SubmitResultDto();
        _mockFixtureService.Setup(s => s.SubmitResultAsync(1, submitDto))
            .ThrowsAsync(new InvalidOperationException("Result exists."));

        // Act
        var result = await _controller.SubmitResult(1, submitDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Result exists.", badRequestResult.Value);
    }
}