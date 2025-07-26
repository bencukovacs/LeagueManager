using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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

    // This test is unchanged and should be passing
    [Fact]
    public async Task GetPlayer_WhenPlayerExists_ReturnsOkResult()
    {
        // Arrange
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);
        // Act
        var result = await _controller.GetPlayer(1);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayer = Assert.IsType<PlayerResponseDto>(okResult.Value);
        Assert.Equal(1, returnedPlayer.Id);
    }

    [Fact]
    public async Task CreatePlayer_WithValidDtoAndAuthorization_ReturnsCreatedAtAction()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };
        var createdPlayer = new PlayerResponseDto { Id = 1, Name = "New Player", TeamId = 1, TeamName = "Team A" };

        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto)).ReturnsAsync(createdPlayer);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedPlayer = Assert.IsType<PlayerResponseDto>(createdAtActionResult.Value);
        Assert.Equal("New Player", returnedPlayer.Name);
    }

    [Fact]
    public async Task CreatePlayer_WhenAuthorizationFails_ReturnsForbid()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdatePlayer_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        var dto = new PlayerDto { Name = "Updated Name", TeamId = 1 };
        var playerResponse = new PlayerResponseDto { Id = 1, Name = "Old Name", TeamId = 1, TeamName = "Team A" };
        var updatedResponse = new PlayerResponseDto { Id = 1, Name = "Updated Name", TeamId = 1, TeamName = "Team A" };

        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerResponse);
        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(1, dto)).ReturnsAsync(updatedResponse);
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _controller.UpdatePlayer(1, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeletePlayer_WhenDeleteIsSuccessful_ReturnsNoContent()
    {
        // Arrange
        var playerResponse = new PlayerResponseDto { Id = 1, Name = "Player to Delete", TeamId = 1, TeamName = "Team A" };

        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerResponse);
        _mockPlayerService.Setup(s => s.DeletePlayerAsync(1)).ReturnsAsync(true);
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _controller.DeletePlayer(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}