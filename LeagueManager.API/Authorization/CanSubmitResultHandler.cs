using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeagueManager.API.Authorization;

public class CanSubmitResultHandler : AuthorizationHandler<CanSubmitResultRequirement, Fixture>
{
    private readonly LeagueDbContext _context;

    public CanSubmitResultHandler(LeagueDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanSubmitResultRequirement requirement,
        Fixture fixture) // The resource is the Fixture object
    {
        // Rule 1: Is the user a global Admin?
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return; // Not authenticated
        }

        // Rule 2: Check if the user is a manager of either the home or away team.
        var isTeamManager = await _context.TeamMemberships
            .AnyAsync(m => (m.TeamId == fixture.HomeTeamId || m.TeamId == fixture.AwayTeamId) &&
                           m.UserId == userId &&
                           (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        if (isTeamManager)
        {
            context.Succeed(requirement);
        }
    }
}