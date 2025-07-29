using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class LeagueTableControllerTests
{
    private readonly Mock<ILeagueTableService> _mockLeagueTableService;
    private readonly LeagueTableController _controller;

    public LeagueTableControllerTests()
    {
        _mockLeagueTableService = new Mock<ILeagueTableService>();
        _controller = new LeagueTableController(_mockLeagueTableService.Object);
    }

    [Fact]
    public async Task Get_WhenServiceReturnsData_ReturnsOkResultWithTable()
    {
        // Arrange
        var fakeTable = new List<TeamStatsDto>
        {
            new TeamStatsDto { TeamName = "Team A", Points = 10 },
            new TeamStatsDto { TeamName = "Team B", Points = 8 }
        };

        _mockLeagueTableService
            .Setup(service => service.GetLeagueTableAsync())
            .ReturnsAsync(fakeTable);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTable = Assert.IsAssignableFrom<IEnumerable<TeamStatsDto>>(okResult.Value);
        Assert.Equal(2, returnedTable.Count());
    }

    [Fact]
    public async Task Get_WhenServiceReturnsEmptyList_ReturnsOkResultWithEmptyTable()
    {
        // Arrange
        var fakeEmptyTable = new List<TeamStatsDto>();

        _mockLeagueTableService
            .Setup(service => service.GetLeagueTableAsync())
            .ReturnsAsync(fakeEmptyTable);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTable = Assert.IsAssignableFrom<IEnumerable<TeamStatsDto>>(okResult.Value);
        Assert.Empty(returnedTable);
    }
}