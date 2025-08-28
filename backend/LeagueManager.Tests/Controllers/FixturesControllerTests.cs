using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
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

    private FixtureResponseDto CreateFixtureDto(int id = 1)
    {
        return new FixtureResponseDto
        {
            Id = id,
            HomeTeam = new TeamResponseDto { Id = 1, Name = "Team A", Status = "Approved" },
            AwayTeam = new TeamResponseDto { Id = 2, Name = "Team B", Status = "Approved" },
            Status = "Scheduled"
        };
    }

    [Fact]
    public async Task GetFixtures_ReturnsOkResult_WithListOfFixtures()
    {
        var fixtureDtoList = new List<FixtureResponseDto> { CreateFixtureDto() };
        _mockFixtureService.Setup(s => s.GetAllFixturesAsync()).ReturnsAsync(fixtureDtoList);

        var result = await _controller.GetFixtures();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFixtures = Assert.IsAssignableFrom<IEnumerable<FixtureResponseDto>>(okResult.Value);
        Assert.Single(returnedFixtures);
    }

    [Fact]
    public async Task GetFixture_WhenFixtureExists_ReturnsOkResult()
    {
        var fixtureDto = CreateFixtureDto();
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);

        var result = await _controller.GetFixture(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFixture = Assert.IsType<FixtureResponseDto>(okResult.Value);
        Assert.Equal(1, returnedFixture.Id);
    }

    [Fact]
    public async Task GetFixture_WhenFixtureDoesNotExist_ReturnsNotFound()
    {
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(99)).ReturnsAsync((FixtureResponseDto?)null);

        var result = await _controller.GetFixture(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetMomVotes_WhenVotesExist_ReturnsOkResultWithVotes()
    {
        var fakeVotes = new List<MomVoteResponseDto>
        {
            new MomVoteResponseDto
            {
                VotingTeamName = "Team A",
                VotedForOwnPlayerName = "Player A1",
                VotedForOpponentPlayerName = "Player B1"
            }
        };

        _mockFixtureService
            .Setup(s => s.GetMomVotesForFixtureAsync(1))
            .ReturnsAsync(fakeVotes);

        var result = await _controller.GetMomVotes(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVotes = Assert.IsAssignableFrom<IEnumerable<MomVoteResponseDto>>(okResult.Value);
        Assert.Single(returnedVotes);
    }

    [Fact]
    public async Task GetMomVotes_WhenNoVotesExist_ReturnsOkResultWithEmptyList()
    {
        var fakeEmptyVotes = new List<MomVoteResponseDto>();
        _mockFixtureService
            .Setup(s => s.GetMomVotesForFixtureAsync(1))
            .ReturnsAsync(fakeEmptyVotes);

        var result = await _controller.GetMomVotes(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVotes = Assert.IsAssignableFrom<IEnumerable<MomVoteResponseDto>>(okResult.Value);
        Assert.Empty(returnedVotes);
    }

    [Fact]
    public async Task CreateFixture_WhenSuccessful_ReturnsCreatedAtAction()
    {
        var createDto = new CreateFixtureDto { HomeTeamId = 1, AwayTeamId = 2 };
        var responseDto = CreateFixtureDto(1);
        _mockFixtureService.Setup(s => s.CreateFixtureAsync(createDto)).ReturnsAsync(responseDto);

        var result = await _controller.CreateFixture(createDto);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetFixture", createdAtActionResult.ActionName);
        Assert.Equal(1, ((FixtureResponseDto)createdAtActionResult.Value!).Id);
    }

    [Fact]
    public async Task CreateFixture_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var createDto = new CreateFixtureDto { HomeTeamId = 1, AwayTeamId = 1 }; // invalid same team
        _mockFixtureService
            .Setup(s => s.CreateFixtureAsync(createDto))
            .ThrowsAsync(new ArgumentException("Teams cannot be the same."));

        var result = await _controller.CreateFixture(createDto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Teams cannot be the same.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateFixture_WhenSuccessful_ReturnsNoContent()
    {
        var updateDto = new UpdateFixtureDto();
        var responseDto = CreateFixtureDto(1);
        _mockFixtureService.Setup(s => s.UpdateFixtureAsync(1, updateDto)).ReturnsAsync(responseDto);

        var result = await _controller.UpdateFixture(1, updateDto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateFixture_WhenFixtureNotFound_ReturnsNotFound()
    {
        var updateDto = new UpdateFixtureDto();
        _mockFixtureService.Setup(s => s.UpdateFixtureAsync(99, updateDto)).ReturnsAsync((FixtureResponseDto?)null);

        var result = await _controller.UpdateFixture(99, updateDto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateFixture_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var updateDto = new UpdateFixtureDto();
        _mockFixtureService
            .Setup(s => s.UpdateFixtureAsync(1, updateDto))
            .ThrowsAsync(new ArgumentException("Invalid fixture data."));

        var result = await _controller.UpdateFixture(1, updateDto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid fixture data.", badRequest.Value);
    }

    [Fact]
    public async Task DeleteFixture_WhenSuccessful_ReturnsNoContent()
    {
        _mockFixtureService.Setup(s => s.DeleteFixtureAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteFixture(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteFixture_WhenNotFound_ReturnsNotFound()
    {
        _mockFixtureService.Setup(s => s.DeleteFixtureAsync(99)).ReturnsAsync(false);

        var result = await _controller.DeleteFixture(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SubmitResult_WhenAuthorizationSucceeds_ReturnsOkResult()
    {
        var submitDto = new SubmitResultDto();
        var fixtureDto = CreateFixtureDto(1);
        var responseDto = new ResultResponseDto { Id = 1, FixtureId = 1, HomeScore = 2, AwayScore = 1, Status = "PendingApproval" };

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockFixtureService.Setup(s => s.SubmitResultAsync(1, submitDto)).ReturnsAsync(responseDto);

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _controller.SubmitResult(1, submitDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<ResultResponseDto>(okResult.Value);
        Assert.Equal(1, returnedResult.Id);
    }

    [Fact]
    public async Task SubmitResult_WhenFixtureNotFound_ReturnsNotFound()
    {
        var submitDto = new SubmitResultDto();
        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(99)).ReturnsAsync((FixtureResponseDto?)null);

        var result = await _controller.SubmitResult(99, submitDto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Fixture not found.", notFound.Value);
    }

    [Fact]
    public async Task SubmitResult_WhenAuthorizationFails_ReturnsForbid()
    {
        var submitDto = new SubmitResultDto();
        var fixtureDto = CreateFixtureDto(1);

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Failed());

        var result = await _controller.SubmitResult(1, submitDto);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task SubmitResult_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var submitDto = new SubmitResultDto { MomVote = new MomVoteDto { VotedForOwnPlayerId = 99, VotedForOpponentPlayerId = 98 } };
        var fixtureDto = CreateFixtureDto(1);

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockFixtureService
            .Setup(s => s.SubmitResultAsync(1, submitDto))
            .ThrowsAsync(new ArgumentException("Invalid player ID in MOM vote."));

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _controller.SubmitResult(1, submitDto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid player ID in MOM vote.", badRequest.Value);
    }

    [Fact]
    public async Task SubmitResult_WhenServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        var submitDto = new SubmitResultDto();
        var fixtureDto = CreateFixtureDto(1);

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockFixtureService
            .Setup(s => s.SubmitResultAsync(1, submitDto))
            .ThrowsAsync(new KeyNotFoundException("Player not found."));

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _controller.SubmitResult(1, submitDto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Player not found.", notFound.Value);
    }

    [Fact]
    public async Task SubmitResult_WhenServiceThrowsInvalidOperationException_ReturnsBadRequest()
    {
        var submitDto = new SubmitResultDto();
        var fixtureDto = CreateFixtureDto(1);

        _mockFixtureService.Setup(s => s.GetFixtureByIdAsync(1)).ReturnsAsync(fixtureDto);
        _mockFixtureService
            .Setup(s => s.SubmitResultAsync(1, submitDto))
            .ThrowsAsync(new InvalidOperationException("Result already submitted."));

        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanSubmitResult"))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await _controller.SubmitResult(1, submitDto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Result already submitted.", badRequest.Value);
    }
}
