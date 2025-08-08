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

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

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
        Assert.IsType<PlayerResponseDto>(okResult.Value);
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
    public async Task CreatePlayer_WhenServiceThrowsUnauthorized_ReturnsForbidden()
    {
        // Arrange
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };
        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto))
            .ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

        // Act
        var result = await _controller.CreatePlayer(playerDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
        Assert.Equal("Not authorized.", objectResult.Value.GetType().GetProperty("Message")?.GetValue(objectResult.Value, null));
    }

    [Fact]
    public async Task UpdatePlayer_WhenAuthorized_ReturnsOkResult()
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
    public async Task UpdatePlayer_WhenUnauthorized_ReturnsForbid()
    {
        // Arrange
        var dto = new PlayerDto { Name = "Updated Name", TeamId = 1 };
        var playerDomainModel = new Player { Id = 1, Name = "Old Name", TeamId = 1 };
        _mockPlayerService.Setup(s => s.GetDomainPlayerByIdAsync(1)).ReturnsAsync(playerDomainModel);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanUpdatePlayer"))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act
        var result = await _controller.UpdatePlayer(1, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromRoster_WhenAuthorized_ReturnsNoContent()
    {
        // Arrange
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Success());
        _mockPlayerService.Setup(s => s.RemovePlayerFromRosterAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemovePlayerFromRoster(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromRoster_WhenPlayerNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(99)).ReturnsAsync((PlayerResponseDto?)null);

        // Act
        var result = await _controller.RemovePlayerFromRoster(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromRoster_WhenUnauthorized_ReturnsForbid()
    {
        // Arrange
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act
        var result = await _controller.RemovePlayerFromRoster(1);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeletePlayerPermanently_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.DeletePlayerPermanentlyAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePlayerPermanently(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePlayerPermanently_WhenPlayerNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.DeletePlayerPermanentlyAsync(99))
            .ThrowsAsync(new KeyNotFoundException("Player not found."));

        // Act
        var result = await _controller.DeletePlayerPermanently(99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Player not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetUnassignedPlayers_ReturnsOkResult_WithUnassignedPlayers()
    {
        // Arrange
        var unassignedPlayers = new List<PlayerResponseDto>
        {
            new() { Id = 1, Name = "Free Agent 1", TeamId = 0, TeamName = "" },
            new() { Id = 2, Name = "Free Agent 2", TeamId = 0, TeamName = "" }
        };
        _mockPlayerService.Setup(s => s.GetUnassignedPlayersAsync()).ReturnsAsync(unassignedPlayers);

        // Act
        var result = await _controller.GetUnassignedPlayers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayers = Assert.IsAssignableFrom<IEnumerable<PlayerResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedPlayers.Count());
    }

    [Fact]
    public async Task AssignPlayerToTeam_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        var responseDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.AssignPlayerToTeamAsync(1, 1)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.AssignPlayerToTeam(1, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayer = Assert.IsType<PlayerResponseDto>(okResult.Value);
        Assert.Equal(1, returnedPlayer.TeamId);
    }

    [Fact]
    public async Task AssignPlayerToTeam_WhenPlayerOrTeamNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.AssignPlayerToTeamAsync(99, 99)).ReturnsAsync((PlayerResponseDto?)null);

        // Act
        var result = await _controller.AssignPlayerToTeam(99, 99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Player or Team not found.", notFoundResult.Value);
    }
}