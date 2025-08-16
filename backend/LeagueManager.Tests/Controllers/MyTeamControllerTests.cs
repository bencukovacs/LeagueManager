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
    public async Task GetMyTeam_WhenServiceReturnsData_ReturnsOkResult()
    {
        // Arrange
        var teamDto = new TeamResponseDto { Id = 1, Name = "My Team", Status = "Approved" };
        var myTeamDto = new MyTeamResponseDto { Team = teamDto, UserRole = "Leader" };
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 5 };
        var responseDto = new MyTeamAndConfigResponseDto { MyTeam = myTeamDto, Config = configDto };
        
        _mockTeamService.Setup(s => s.GetMyTeamAndConfigAsync()).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.GetMyTeam();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsType<MyTeamAndConfigResponseDto>(okResult.Value);
        Assert.NotNull(returnedData.MyTeam);
        Assert.Equal(1, returnedData.MyTeam.Team.Id);
        Assert.Equal("Leader", returnedData.MyTeam.UserRole);
    }

    [Fact]
    public async Task GetMyTeam_WhenUserIsNotOnTeam_ReturnsOkWithNullTeam()
    {
        // Arrange
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 5 };
        // The service returns the config, but the MyTeam property is null
        var responseDto = new MyTeamAndConfigResponseDto { MyTeam = null, Config = configDto };
        _mockTeamService.Setup(s => s.GetMyTeamAndConfigAsync()).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.GetMyTeam();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsType<MyTeamAndConfigResponseDto>(okResult.Value);
        Assert.Null(returnedData.MyTeam); // Verify that the MyTeam part of the response is null
        Assert.NotNull(returnedData.Config);
    }

    [Fact]
    public async Task GetMyTeamFixtures_WhenServiceReturnsFixtures_ReturnsOkResult()
    {
        // Arrange
        var fixtureList = new List<FixtureResponseDto>
        {
            new FixtureResponseDto { Id = 1, HomeTeam = new TeamResponseDto { Id = 1, Name = "A", Status = "Approved" }, AwayTeam = new TeamResponseDto { Id = 2, Name = "B", Status = "Approved" }, Status = "Scheduled", HomeTeamRoster = new(), AwayTeamRoster = new() }
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
    
    [Fact]
    public async Task LeaveMyTeam_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        _mockTeamService.Setup(s => s.LeaveMyTeamAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LeaveMyTeam();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task LeaveMyTeam_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var errorMessage = "You are the last manager of this team.";
        _mockTeamService.Setup(s => s.LeaveMyTeamAsync())
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _controller.LeaveMyTeam();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }
}