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
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);

        var result = await _controller.GetPlayer(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<PlayerResponseDto>(okResult.Value);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerDoesNotExist_ReturnsNotFound()
    {
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(99)).ReturnsAsync((PlayerResponseDto?)null);

        var result = await _controller.GetPlayer(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreatePlayer_WhenServiceSucceeds_ReturnsCreatedAtAction()
    {
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };
        var responseDto = new PlayerResponseDto { Id = 1, Name = "New Player", TeamId = 1, TeamName = "Team A" };

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDto.TeamId, "CanEditRoster"))
            .ReturnsAsync(AuthorizationResult.Success());

        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto)).ReturnsAsync(responseDto);

        var result = await _controller.CreatePlayer(playerDto);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetPlayer", createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task CreatePlayer_WhenServiceThrowsUnauthorized_ReturnsForbidden()
    {
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDto.TeamId, "CanEditRoster"))
            .ReturnsAsync(AuthorizationResult.Success());

        _mockPlayerService.Setup(s => s.CreatePlayerAsync(playerDto))
            .ThrowsAsync(new UnauthorizedAccessException("Not authorized."));

        var result = await _controller.CreatePlayer(playerDto);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
        Assert.Equal("Not authorized.", objectResult.Value.GetType().GetProperty("Message")?.GetValue(objectResult.Value, null));
    }

    [Fact]
    public async Task CreatePlayer_WhenAuthorizationFails_ReturnsForbid()
    {
        var playerDto = new PlayerDto { Name = "New Player", TeamId = 1 };

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDto.TeamId, "CanEditRoster"))
            .ReturnsAsync(AuthorizationResult.Failed());

        var result = await _controller.CreatePlayer(playerDto);

        Assert.IsType<ForbidResult>(result);
        _mockPlayerService.Verify(s => s.CreatePlayerAsync(It.IsAny<PlayerDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePlayer_WhenAuthorized_ReturnsOkResult()
    {
        var dto = new PlayerDto { Name = "Updated Name", TeamId = 1 };
        var playerDomainModel = new Player { Id = 1, Name = "Old Name", TeamId = 1 };
        var updatedResponse = new PlayerResponseDto { Id = 1, Name = "Updated Name", TeamId = 1, TeamName = "Team A" };

        _mockPlayerService.Setup(s => s.GetDomainPlayerByIdAsync(1)).ReturnsAsync(playerDomainModel);
        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(1, dto)).ReturnsAsync(updatedResponse);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDomainModel, "CanUpdatePlayer"))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _controller.UpdatePlayer(1, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePlayer_WhenUnauthorized_ReturnsForbid()
    {
        var dto = new PlayerDto { Name = "Updated Name", TeamId = 1 };
        var playerDomainModel = new Player { Id = 1, Name = "Old Name", TeamId = 1 };
        _mockPlayerService.Setup(s => s.GetDomainPlayerByIdAsync(1)).ReturnsAsync(playerDomainModel);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDomainModel, "CanUpdatePlayer"))
            .ReturnsAsync(AuthorizationResult.Failed());

        var result = await _controller.UpdatePlayer(1, dto);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact] 
    public async Task UpdatePlayer_WhenPlayerNotFound_ReturnsNotFound()
    {
        var dto = new PlayerDto { Name = "Updated Name", TeamId = 1 };
        _mockPlayerService.Setup(s => s.GetDomainPlayerByIdAsync(1)).ReturnsAsync((Player?)null);

        var result = await _controller.UpdatePlayer(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromRoster_WhenAuthorized_ReturnsNoContent()
    {
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDto.TeamId, "CanEditRoster"))
            .ReturnsAsync(AuthorizationResult.Success());

        _mockPlayerService.Setup(s => s.RemovePlayerFromRosterAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.RemovePlayerFromRoster(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromRoster_WhenPlayerNotFound_ReturnsNotFound()
    {
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(99)).ReturnsAsync((PlayerResponseDto?)null);

        var result = await _controller.RemovePlayerFromRoster(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromRoster_WhenUnauthorized_ReturnsForbid()
    {
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDto.TeamId, "CanEditRoster"))
            .ReturnsAsync(AuthorizationResult.Failed());

        var result = await _controller.RemovePlayerFromRoster(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromRoster_WhenServiceThrowsInvalidOperation_ReturnsBadRequest()
    {
        var playerDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(playerDto);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), playerDto.TeamId, "CanEditRoster"))
            .ReturnsAsync(AuthorizationResult.Success());

        _mockPlayerService.Setup(s => s.RemovePlayerFromRosterAsync(1))
            .ThrowsAsync(new InvalidOperationException("Already removed."));

        var result = await _controller.RemovePlayerFromRoster(1);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Already removed.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeletePlayerPermanently_WhenSuccessful_ReturnsNoContent()
    {
        _mockPlayerService.Setup(s => s.DeletePlayerPermanentlyAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.DeletePlayerPermanently(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePlayerPermanently_WhenPlayerNotFound_ReturnsNotFound()
    {
        _mockPlayerService.Setup(s => s.DeletePlayerPermanentlyAsync(99))
            .ThrowsAsync(new KeyNotFoundException("Player not found."));

        var result = await _controller.DeletePlayerPermanently(99);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Player not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetUnassignedPlayers_ReturnsOkResult_WithUnassignedPlayers()
    {
        var unassignedPlayers = new List<PlayerResponseDto>
        {
            new() { Id = 1, Name = "Free Agent 1", TeamId = 0, TeamName = "" },
            new() { Id = 2, Name = "Free Agent 2", TeamId = 0, TeamName = "" }
        };
        _mockPlayerService.Setup(s => s.GetUnassignedPlayersAsync()).ReturnsAsync(unassignedPlayers);

        var result = await _controller.GetUnassignedPlayers();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayers = Assert.IsAssignableFrom<IEnumerable<PlayerResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedPlayers.Count());
    }

    [Fact]
    public async Task AssignPlayerToTeam_WhenSuccessful_ReturnsOkResult()
    {
        var responseDto = new PlayerResponseDto { Id = 1, Name = "Test Player", TeamId = 1, TeamName = "Team A" };
        _mockPlayerService.Setup(s => s.AssignPlayerToTeamAsync(1, 1)).ReturnsAsync(responseDto);

        var result = await _controller.AssignPlayerToTeam(1, 1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayer = Assert.IsType<PlayerResponseDto>(okResult.Value);
        Assert.Equal(1, returnedPlayer.TeamId);
    }

    [Fact]
    public async Task AssignPlayerToTeam_WhenPlayerOrTeamNotFound_ReturnsNotFound()
    {
        _mockPlayerService.Setup(s => s.AssignPlayerToTeamAsync(99, 99)).ReturnsAsync((PlayerResponseDto?)null);

        var result = await _controller.AssignPlayerToTeam(99, 99);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Player or Team not found.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task DeletePlayerPermanently_WhenPlayerIsLinkedToUser_ReturnsBadRequest()
    {
        var expectedErrorMessage = "Cannot permanently delete a player who is linked to a registered user account.";
        _mockPlayerService.Setup(s => s.DeletePlayerPermanentlyAsync(1))
            .ThrowsAsync(new InvalidOperationException(expectedErrorMessage));

        var result = await _controller.DeletePlayerPermanently(1);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains(expectedErrorMessage, badRequestResult.Value?.ToString());
    }

    [Fact] 
    public async Task GetPlayers_WhenPlayersExist_ReturnsOkResult()
    {
        var players = new List<PlayerResponseDto>
        {
            new() { Id = 1, Name = "Player 1", TeamId = 1, TeamName = "Team A" },
            new() { Id = 2, Name = "Player 2", TeamId = 2, TeamName = "Team B" }
        };
        _mockPlayerService.Setup(s => s.GetAllPlayersAsync()).ReturnsAsync(players);

        var result = await _controller.GetPlayers();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlayers = Assert.IsAssignableFrom<IEnumerable<PlayerResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedPlayers.Count());
    }
}
