using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeagueManager.API.Authorization;

public class CanManageTeamHandler : AuthorizationHandler<CanManageTeamRequirement, int>
{
    private readonly LeagueDbContext _context;

    public CanManageTeamHandler(LeagueDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanManageTeamRequirement requirement,
        int teamId) // The resource we are protecting is the team's ID
    {
        // Rule 1: Is the user a global Admin? If so, they are always authorized.
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // Get the current user's ID from their claims
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return; // User is not authenticated, so they fail the check.
        }

        // Rule 2: Check the database to see if the user is a Leader or AssistantLeader of this specific team.
        var isTeamManager = await _context.TeamMemberships
            .AnyAsync(m => m.TeamId == teamId &&
                           m.UserId == userId &&
                           (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        if (isTeamManager)
        {
            context.Succeed(requirement);
        }
    }
}