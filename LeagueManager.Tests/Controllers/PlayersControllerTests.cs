using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class PlayersControllerTests
{
    private readonly Mock<IPlayerService> _mockPlayerService;
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _mockPlayerService = new Mock<IPlayerService>();
        _controller = new PlayersController(_mockPlayerService.Object);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerExists_ReturnsOkResult()
    {
        // Arrange
        var player = new Player { Id = 1, Name = "Test Player", TeamId = 1 };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(player);

        // Act
        var result = await _controller.GetPlayer(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayer = Assert.IsType<Player>(okResult.Value);
        Assert.Equal(1, returnedPlayer.Id);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(99)).ReturnsAsync((Player?)null);

        // Act
        var result = await _controller.GetPlayer(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreatePlayer_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };
        var newPlayer = new Player { Id = 1, Name = "New Player", TeamId = 1 };
        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto)).ReturnsAsync(newPlayer);
        // We also need to mock the GetPlayerByIdAsync call that happens inside the action
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(newPlayer.Id)).ReturnsAsync(newPlayer);

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdPlayer = Assert.IsType<Player>(createdAtActionResult.Value);
        Assert.Equal("New Player", createdPlayer.Name);
    }

    [Fact]
    public async Task CreatePlayer_WithInvalidTeamId_ReturnsBadRequest()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 99 };
        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto))
            .ThrowsAsync(new ArgumentException("Invalid Team ID."));

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Team ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeletePlayer_WhenDeleteIsSuccessful_ReturnsNoContent()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.DeletePlayerAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePlayer(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePlayer_WhenPlayerDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.DeletePlayerAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePlayer(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}