using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
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
    public async Task GetTeams_ReturnsOkResult_WithListOfTeams()
    {
        // Arrange
        // FIX: Added Status property
        var teamDtoList = new List<TeamResponseDto> { new() { Id = 1, Name = "Team A", Status = "Approved" } };
        _mockTeamService.Setup(s => s.GetAllTeamsAsync()).ReturnsAsync(teamDtoList);

        // Act
        var result = await _controller.GetTeams();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<TeamResponseDto>>(okResult.Value);
    }

    [Fact]
    public async Task GetTeam_WhenTeamExists_ReturnsOkResult()
    {
        // Arrange
        // FIX: Added Status property
        var teamDto = new TeamResponseDto { Id = 1, Name = "Test Team", Status = "Approved" };
        _mockTeamService.Setup(service => service.GetTeamByIdAsync(1)).ReturnsAsync(teamDto);

        // Act
        var result = await _controller.GetTeam(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTeam = Assert.IsType<TeamResponseDto>(okResult.Value);
        Assert.Equal(1, returnedTeam.Id);
    }

    [Fact]
    public async Task GetTeam_WhenTeamDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockTeamService.Setup(service => service.GetTeamByIdAsync(99)).ReturnsAsync((TeamResponseDto?)null);

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
        // FIX: Added Status property
        var responseDto = new TeamResponseDto { Id = 1, Name = "New Team", Status = "PendingApproval" };
        _mockTeamService.Setup(service => service.CreateTeamAsync(createDto)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.CreateTeam(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetTeam", createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.RouteValues);
        Assert.Equal(1, createdAtActionResult.RouteValues["id"]);
    }

    [Fact]
    public async Task CreateTeam_WhenUserIsUnauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var createDto = new CreateTeamDto { Name = "New Team" };
        _mockTeamService.Setup(service => service.CreateTeamAsync(createDto))
            .ThrowsAsync(new UnauthorizedAccessException("User is not authenticated."));

        // Act
        var result = await _controller.CreateTeam(createDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User is not authenticated.", unauthorizedResult.Value);
    }
    
    [Fact]
    public async Task ApproveTeam_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        // FIX: Added Status property
        var responseDto = new TeamResponseDto { Id = 1, Name = "Approved Team", Status = "Approved" };
        _mockTeamService.Setup(s => s.ApproveTeamAsync(1)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.ApproveTeam(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTeam = Assert.IsType<TeamResponseDto>(okResult.Value);
        Assert.Equal("Approved", returnedTeam.Status);
    }

    [Fact]
    public async Task ApproveTeam_WhenTeamNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockTeamService.Setup(s => s.ApproveTeamAsync(99)).ReturnsAsync((TeamResponseDto?)null);

        // Act
        var result = await _controller.ApproveTeam(99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Team not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateTeam_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var updateDto = new CreateTeamDto { Name = "Updated Name" };
        // FIX: Added Status property
        var responseDto = new TeamResponseDto { Id = 1, Name = "Updated Name", Status = "Approved" };
        _mockTeamService.Setup(s => s.UpdateTeamAsync(1, updateDto)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.UpdateTeam(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateTeam_WhenTeamNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new CreateTeamDto { Name = "Updated Name" };
        _mockTeamService.Setup(s => s.UpdateTeamAsync(99, updateDto)).ReturnsAsync((TeamResponseDto?)null);

        // Act
        var result = await _controller.UpdateTeam(99, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
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
    public async Task DeleteTeam_WhenTeamIsInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        var expectedExceptionMessage = "Cannot delete a team that is currently assigned to a fixture.";
        _mockTeamService.Setup(service => service.DeleteTeamAsync(1))
            .ThrowsAsync(new InvalidOperationException(expectedExceptionMessage));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteTeam(1));
        
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }
}