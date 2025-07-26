using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LeagueManager.Domain.Models;

namespace LeagueManager.Tests.Controllers;

public class PlayersControllerTests
{
    private readonly Mock<IPlayerService> _mockPlayerService;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _mockPlayerService = new Mock<IPlayerService>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _controller = new PlayersController(_mockPlayerService.Object, _mockAuthorizationService.Object);

        // Set up a default user for the controller's context
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    [Fact]
    public async Task CreatePlayer_WhenServiceSucceeds_ReturnsCreatedAtAction()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };
        var responseDto = new PlayerResponseDto { Id = 1, Name = "New Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetPlayer", createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task CreatePlayer_WhenServiceThrowsUnauthorized_ReturnsForbid()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };
        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto))
            .ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdatePlayer_WhenAuthorizationSucceeds_ReturnsOkResult()
    {
        // Arrange
        var dto = new PlayerDto { Name = "Updated Name", TeamId = 1 };
        var playerDomainModel = new Player { Id = 1, Name = "Old Name", TeamId = 1 };
        var updatedResponse = new PlayerResponseDto { Id = 1, Name = "Updated Name", TeamId = 1, TeamName = "Team A" };

        _mockPlayerService.Setup(s => s.GetDomainPlayerByIdAsync(1)).ReturnsAsync(playerDomainModel);
        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(1, dto)).ReturnsAsync(updatedResponse);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanUpdatePlayer"))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _controller.UpdatePlayer(1, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeletePlayer_WhenServiceSucceeds_ReturnsNoContent()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.DeletePlayerAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePlayer(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePlayer_WhenPlayerNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.DeletePlayerAsync(99))
            .ThrowsAsync(new KeyNotFoundException("Player not found."));

        // Act
        var result = await _controller.DeletePlayer(99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Player not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task DeletePlayer_WhenUnauthorized_ReturnsForbid()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.DeletePlayerAsync(1))
            .ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

        // Act
        var result = await _controller.DeletePlayer(1);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
    }
}