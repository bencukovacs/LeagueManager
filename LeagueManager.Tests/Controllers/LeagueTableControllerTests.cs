using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.API.Services;
using LeagueManager.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class LeagueTableControllerTests
{
    private readonly Mock<ILeagueTableService> _mockLeagueTableService;
    private readonly LeagueTableController _controller;

    public LeagueTableControllerTests()
    {
        // 1. Create a mock object of the service interface
        _mockLeagueTableService = new Mock<ILeagueTableService>();
        
        // 2. Create an instance of the controller, passing in the mocked object
        _controller = new LeagueTableController(_mockLeagueTableService.Object);
    }

    [Fact]
    public async Task Get_WhenServiceReturnsData_ReturnsOkResultWithTable()
    {
        // --- ARRANGE ---
        // Create some fake data that we want our mock service to return
        var fakeTable = new List<TeamStatsDto>
        {
            new TeamStatsDto { TeamName = "Team A", Points = 10 },
            new TeamStatsDto { TeamName = "Team B", Points = 8 }
        };

        // Configure the mock: "When the GetLeagueTableAsync method is called,
        // return our fake data."
        _mockLeagueTableService
            .Setup(service => service.GetLeagueTableAsync())
            .ReturnsAsync(fakeTable);

        // --- ACT ---
        // Call the controller method. The controller will call our fake service.
        var result = await _controller.Get();

        // --- ASSERT ---
        // Check that the controller returned a 200 OK response
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        // Check that the data inside the response is the fake data we set up
        var returnedTable = Assert.IsAssignableFrom<IEnumerable<TeamStatsDto>>(okResult.Value);
        Assert.Equal(2, returnedTable.Count());
    }

    [Fact]
    public async Task Get_WhenServiceReturnsEmptyList_ReturnsOkResultWithEmptyTable()
    {
        // --- ARRANGE ---
        var fakeEmptyTable = new List<TeamStatsDto>();

        // Configure the mock to return an empty list
        _mockLeagueTableService
            .Setup(service => service.GetLeagueTableAsync())
            .ReturnsAsync(fakeEmptyTable);

        // --- ACT ---
        var result = await _controller.Get();

        // --- ASSERT ---
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTable = Assert.IsAssignableFrom<IEnumerable<TeamStatsDto>>(okResult.Value);
        Assert.Empty(returnedTable);
    }
}