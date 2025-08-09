using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class MeControllerTests
{
    private readonly Mock<IRosterRequestService> _mockRosterRequestService;
    private readonly MeController _controller;

    public MeControllerTests()
    {
        _mockRosterRequestService = new Mock<IRosterRequestService>();
        _controller = new MeController(_mockRosterRequestService.Object);
    }

    [Fact]
    public async Task GetMyPendingRequests_WhenServiceReturnsRequests_ReturnsOkResult()
    {
        // Arrange
        var requestList = new List<RosterRequestResponseDto>
        {
            new RosterRequestResponseDto { Id = 1, UserName = "test", TeamName = "Team A", Type = "JoinRequest", Status = "PendingLeaderApproval" }
        };
        _mockRosterRequestService.Setup(s => s.GetMyPendingRequestsAsync()).ReturnsAsync(requestList);

        // Act
        var result = await _controller.GetMyPendingRequests();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRequests = Assert.IsAssignableFrom<IEnumerable<RosterRequestResponseDto>>(okResult.Value);
        Assert.Single(returnedRequests);
    }
}