using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
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
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);

        // Act
        var result = await _controller.GetPlayer(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayer = Assert.IsType<PlayerResponseDto>(okResult.Value);
        Assert.Equal(1, returnedPlayer.Id);
        Assert.Equal("Test Player", returnedPlayer.Name);
        Assert.Equal("Team A", returnedPlayer.TeamName);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(99)).ReturnsAsync((PlayerResponseDto?)null);

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
        var createdPlayer = new PlayerResponseDto { Id = 1, Name = "New Player", TeamName = "Team A" };

        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto)).ReturnsAsync(createdPlayer);

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedPlayer = Assert.IsType<PlayerResponseDto>(createdAtActionResult.Value);
        Assert.Equal("New Player", returnedPlayer.Name);
        Assert.Equal("Team A", returnedPlayer.TeamName);
    }

    [Fact]
    public async Task CreatePlayer_WithInvalidTeamId_ReturnsBadRequest()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 99 };
        _mockPlayerService
            .Setup(s => s.CreatePlayerAsync(playerDto))
            .ThrowsAsync(new ArgumentException("Invalid Team ID."));

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Team ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdatePlayer_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var dto = new PlayerDto { Name = "Updated Name" };
        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(1, dto))
                            .ReturnsAsync(new PlayerResponseDto { Id = 1, Name = "Updated Name" });

        // Act
        var result = await _controller.UpdatePlayer(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdatePlayer_WhenPlayerNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new PlayerDto { Name = "Updated Name" };
        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(99, dto)).ReturnsAsync((PlayerResponseDto?)null);

        // Act
        var result = await _controller.UpdatePlayer(99, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
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
