using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class MyTeamControllerTests
{
    private readonly Mock<ITeamService> _mockTeamService;
    private readonly MyTeamController _controller;

    public MyTeamControllerTests()
    {
        _mockTeamService = new Mock<ITeamService>();
        _controller = new MyTeamController(_mockTeamService.Object);
    }

    [Fact]
    public async Task GetMyTeam_WhenServiceReturnsTeam_ReturnsOkResult()
    {
        // Arrange
        var teamDto = new TeamResponseDto { Id = 1, Name = "My Team", Status = "Approved" };
        var myTeamResponseDto = new MyTeamResponseDto { Team = teamDto, UserRole = "Leader" };
        _mockTeamService.Setup(s => s.GetMyTeamAsync()).ReturnsAsync(myTeamResponseDto);

        // Act
        var result = await _controller.GetMyTeam();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsType<MyTeamResponseDto>(okResult.Value);
        Assert.Equal(1, returnedData.Team.Id);
        Assert.Equal("Leader", returnedData.UserRole);
    }

    [Fact]
    public async Task GetMyTeam_WhenServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        _mockTeamService.Setup(s => s.GetMyTeamAsync()).ReturnsAsync((MyTeamResponseDto?)null);

        // Act
        var result = await _controller.GetMyTeam();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("You do not currently have a team.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetMyTeamFixtures_WhenServiceReturnsFixtures_ReturnsOkResult()
    {
        // Arrange
        var fixtureList = new List<FixtureResponseDto>
        {
            new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled" }
        };
        _mockTeamService.Setup(s => s.GetFixturesForMyTeamAsync()).ReturnsAsync(fixtureList);

        // Act
        var result = await _controller.GetMyTeamFixtures();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFixtures = Assert.IsAssignableFrom<IEnumerable<FixtureResponseDto>>(okResult.Value);
        Assert.Single(returnedFixtures);
    }

    [Fact]
    public async Task GetMyTeamFixtures_WhenServiceReturnsEmpty_ReturnsOkResultWithEmptyList()
    {
        // Arrange
        _mockTeamService.Setup(s => s.GetFixturesForMyTeamAsync()).ReturnsAsync(new List<FixtureResponseDto>());

        // Act
        var result = await _controller.GetMyTeamFixtures();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedFixtures = Assert.IsAssignableFrom<IEnumerable<FixtureResponseDto>>(okResult.Value);
        Assert.Empty(returnedFixtures);
    }
}