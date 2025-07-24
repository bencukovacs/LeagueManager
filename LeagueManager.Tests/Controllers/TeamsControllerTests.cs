using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.API.Services;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class TeamsControllerTests
{
    private readonly Mock<ITeamService> _mockTeamService;
    private readonly TeamsController _controller;

    public TeamsControllerTests()
    {
        _mockTeamService = new Mock<ITeamService>();
        _controller = new TeamsController(_mockTeamService.Object);
    }

    [Fact]
    public async Task GetTeam_WhenTeamExists_ReturnsOkResult()
    {
        // Arrange
        var team = new Team { Id = 1, Name = "Test Team" };
        _mockTeamService.Setup(service => service.GetTeamByIdAsync(1)).ReturnsAsync(team);

        // Act
        var result = await _controller.GetTeam(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTeam = Assert.IsType<Team>(okResult.Value);
        Assert.Equal(1, returnedTeam.Id);
    }

    [Fact]
    public async Task GetTeam_WhenTeamDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockTeamService.Setup(service => service.GetTeamByIdAsync(99)).ReturnsAsync((Team?)null);

        // Act
        var result = await _controller.GetTeam(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateTeam_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateTeamDto { Name = "New Team" };
        var newTeam = new Team { Id = 1, Name = "New Team" };
        _mockTeamService.Setup(service => service.CreateTeamAsync(createDto)).ReturnsAsync(newTeam);

        // Act
        var result = await _controller.CreateTeam(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetTeam", createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.RouteValues);
        Assert.Equal(1, createdAtActionResult.RouteValues["id"]);
    }

    [Fact]
    public async Task DeleteTeam_WhenDeleteIsSuccessful_ReturnsNoContent()
    {
        // Arrange
        _mockTeamService.Setup(service => service.DeleteTeamAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTeam(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTeam_WhenTeamDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockTeamService.Setup(service => service.DeleteTeamAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTeam(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteTeam_WhenTeamIsInUse_ReturnsBadRequest()
    {
        // Arrange
        _mockTeamService.Setup(service => service.DeleteTeamAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete a team that is currently assigned to a fixture."));

        // Act
        var result = await _controller.DeleteTeam(1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Cannot delete a team that is currently assigned to a fixture.", badRequestResult.Value);
    }
}