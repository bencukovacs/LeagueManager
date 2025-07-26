using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LeagueManager.Tests.Controllers;

public class FixturesControllerTests
{
    private readonly Mock<IFixtureService> _mockFixtureService;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly FixturesController _controller;

    public FixturesControllerTests()
    {
        _mockFixtureService = new Mock<IFixtureService>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _controller = new FixturesController(_mockFixtureService.Object, _mockAuthorizationService.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
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
        var fixtureDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled" };
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);

        // Act
        var result = await _controller.GetFixture(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<FixtureResponseDto>(okResult.Value);
    }

    [Fact]
    public async Task CreateFixture_WhenSuccessful_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateFixtureDto { HomeTeamId = 1, AwayTeamId = 2 };
        var responseDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled" };
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
        var responseDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled" };
        _mockFixtureService.Setup(s => s.UpdateFixtureAsync(1, updateDto)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.UpdateFixture(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteFixture_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        _mockFixtureService.Setup(s => s.DeleteFixtureAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFixture(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SubmitResult_WhenAuthorizationSucceeds_ReturnsOkResult()
    {
        // Arrange
        var submitDto = new SubmitResultDto();
        var fixtureDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled" };
        var newResult = new Result { Id = 1, FixtureId = 1 };

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockFixtureService.Setup(s => s.SubmitResultAsync(1, submitDto)).ReturnsAsync(newResult);
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _controller.SubmitResult(1, submitDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SubmitResult_WhenAuthorizationFails_ReturnsForbid()
    {
        // Arrange
        var submitDto = new SubmitResultDto();
        var fixtureDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled" };

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act
        var result = await _controller.SubmitResult(1, submitDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}