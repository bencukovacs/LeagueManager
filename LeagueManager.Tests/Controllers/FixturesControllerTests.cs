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
        // --- ARRANGE ---
// 1. Create the teams and fixture
var teamA = new Team { Id = 1, Name = "Team A" };
var teamB = new Team { Id = 2, Name = "Team B" };
var fixture = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };

// 2. Create players for EACH team
var playerFromTeamA = new Player { Id = 10, Name = "Player A", TeamId = 1 };
var playerFromTeamB = new Player { Id = 20, Name = "Player B", TeamId = 2 };

// 3. Create a user who is the leader of Team A
var user = new User { Id = "user-123" };
var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };

// 4. Create the DTO with the CORRECT player IDs
var submitDto = new SubmitResultDto
{
    HomeScore = 1,
    AwayScore = 0,
    MomVote = new MomVoteDto 
    {
        VotedForOwnPlayerId = 10,     // Player from Team A (submitter's team)
        VotedForOpponentPlayerId = 20 // Player from Team B (opponent's team)
    }
};
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
    public async Task SubmitResult_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var submitDto = new SubmitResultDto { MomVote = new MomVoteDto { VotedForOwnPlayerId = 99, VotedForOpponentPlayerId = 98 }}; // Invalid player IDs
        var fixtureDto = new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled" };

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockFixtureService.Setup(s => s.SubmitResultAsync(1, submitDto))
            .ThrowsAsync(new ArgumentException("Invalid player ID in MOM vote."));
        
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _controller.SubmitResult(1, submitDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid player ID in MOM vote.", badRequestResult.Value);
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