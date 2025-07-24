using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class TopScorersControllerTests
{
    private readonly Mock<ITopScorersService> _mockTopScorersService;
    private readonly TopScorersController _controller;

    public TopScorersControllerTests()
    {
        // 1. Create a mock of the service interface
        _mockTopScorersService = new Mock<ITopScorersService>();
        
        // 2. Create an instance of the controller with the mock
        _controller = new TopScorersController(_mockTopScorersService.Object);
    }

    [Fact]
    public async Task GetTopScorers_WhenServiceReturnsData_ReturnsOkResultWithScorers()
    {
        // --- ARRANGE ---
        // Create fake data for the mock service to return
        var fakeScorers = new List<TopScorerDto>
        {
            new TopScorerDto { PlayerName = "Player A", TeamName = "Team X", Goals = 15 },
            new TopScorerDto { PlayerName = "Player B", TeamName = "Team Y", Goals = 12 }
        };

        // Configure the mock to return the fake data
        _mockTopScorersService
            .Setup(service => service.GetTopScorersAsync())
            .ReturnsAsync(fakeScorers);

        // --- ACT ---
        var result = await _controller.GetTopScorers();

        // --- ASSERT ---
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedScorers = Assert.IsAssignableFrom<IEnumerable<TopScorerDto>>(okResult.Value);
        Assert.Equal(2, returnedScorers.Count());
    }

    [Fact]
    public async Task GetTopScorers_WhenServiceReturnsEmptyList_ReturnsOkResultWithEmptyList()
    {
        // --- ARRANGE ---
        var fakeEmptyList = new List<TopScorerDto>();

        // Configure the mock to return an empty list
        _mockTopScorersService
            .Setup(service => service.GetTopScorersAsync())
            .ReturnsAsync(fakeEmptyList);

        // --- ACT ---
        var result = await _controller.GetTopScorers();

        // --- ASSERT ---
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedScorers = Assert.IsAssignableFrom<IEnumerable<TopScorerDto>>(okResult.Value);
        Assert.Empty(returnedScorers);
    }
}