using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/teams/{teamId}/members")]
[Authorize]
public class TeamMembershipsController : ControllerBase
{
    private readonly ITeamMembershipService _membershipService;
    private readonly IAuthorizationService _authorizationService;

    public TeamMembershipsController(ITeamMembershipService membershipService, IAuthorizationService authorizationService)
    {
        _membershipService = membershipService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTeamMembers(int teamId)
    {
        // Authorization: Can the current user view the members of this team?
        // For now, we'll assume any logged-in user can, but a policy could be added here.
        var members = await _membershipService.GetMembersForTeamAsync(teamId);
        return Ok(members);
    }

    [HttpPut("{userId}/role")]
    public async Task<IActionResult> UpdateMemberRole(int teamId, string userId, [FromBody] UpdateTeamMemberRoleDto dto)
    {
        // Authorization: Can the current user manage this team?
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, teamId, "CanManageTeam");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var updatedMember = await _membershipService.UpdateMemberRoleAsync(teamId, userId, dto);
        if (updatedMember == null)
        {
            return NotFound("Team membership not found.");
        }

        return Ok(updatedMember);
    }
    
    // Add this new endpoint to your controller
    [HttpPatch("{userId}/demote")]
    public async Task<IActionResult> DemoteMember(int teamId, string userId)
    {
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, teamId, "CanManageTeam");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var dto = new UpdateTeamMemberRoleDto { NewRole = TeamRole.Member };
        var updatedMember = await _membershipService.UpdateMemberRoleAsync(teamId, userId, dto);
        if (updatedMember == null)
        {
            return NotFound("Team membership not found.");
        }

        return Ok(updatedMember);
    }
}