using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeagueManager.API.Authorization;

public class CanEditRosterHandler : AuthorizationHandler<CanEditRosterRequirement, int>
{
    private readonly LeagueDbContext _context;

    public CanEditRosterHandler(LeagueDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanEditRosterRequirement requirement,
        int teamId)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return;
        }

        // This is the key difference: it allows both Leader and AssistantLeader
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