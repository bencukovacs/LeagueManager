using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LeagueManager.Domain.Models;

namespace LeagueManager.Tests.Controllers;

public class TeamMembershipsControllerTests
{
    private readonly Mock<ITeamMembershipService> _mockMembershipService;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly TeamMembershipsController _controller;

    public TeamMembershipsControllerTests()
    {
        _mockMembershipService = new Mock<ITeamMembershipService>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _controller = new TeamMembershipsController(_mockMembershipService.Object, _mockAuthorizationService.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    [Fact]
    public async Task GetTeamMembers_ReturnsOkResult_WithMembers()
    {
        // Arrange
        var members = new List<TeamMemberResponseDto> { new() { UserId = "1", Email = "test@test.hu", FullName = "Test User", Role = "Leader" } };
        _mockMembershipService.Setup(s => s.GetMembersForTeamAsync(1)).ReturnsAsync(members);

        // Act
        var result = await _controller.GetTeamMembers(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<TeamMemberResponseDto>>(okResult.Value);
    }

    [Fact]
    public async Task UpdateMemberRole_WhenAuthorized_ReturnsOkResult()
    {
        // Arrange
        var dto = new UpdateTeamMemberRoleDto { NewRole = TeamRole.AssistantLeader };
        var responseDto = new TeamMemberResponseDto { UserId = "user-2", Email = "test2@test.hu", FullName = "User 2", Role = "AssistantLeader" };
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Success());
        _mockMembershipService.Setup(s => s.UpdateMemberRoleAsync(1, "user-2", dto)).ReturnsAsync(responseDto);

        // Act
        var result = await _controller.UpdateMemberRole(1, "user-2", dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<TeamMemberResponseDto>(okResult.Value);
    }

    [Fact]
    public async Task UpdateMemberRole_WhenUnauthorized_ReturnsForbid()
    {
        // Arrange
        var dto = new UpdateTeamMemberRoleDto();
        _mockAuthorizationService
            .Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanManageTeam"))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act
        var result = await _controller.UpdateMemberRole(1, "user-2", dto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}