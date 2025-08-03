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
        _mockTeamService.Setup(s => s.GetMyTeamAsync()).ReturnsAsync(teamDto);

        // Act
        var result = await _controller.GetMyTeam();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTeam = Assert.IsType<TeamResponseDto>(okResult.Value);
        Assert.Equal(1, returnedTeam.Id);
    }

    [Fact]
    public async Task GetMyTeam_WhenServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        _mockTeamService.Setup(s => s.GetMyTeamAsync()).ReturnsAsync((TeamResponseDto?)null);

        // Act
        var result = await _controller.GetMyTeam();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("You do not currently have a team.", notFoundResult.Value);
    }
}