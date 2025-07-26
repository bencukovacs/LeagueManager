using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeagueManager.API.Authorization;

public class CanUpdatePlayerHandler : AuthorizationHandler<CanUpdatePlayerRequirement, Player>
{
    private readonly LeagueDbContext _context;

    public CanUpdatePlayerHandler(LeagueDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanUpdatePlayerRequirement requirement,
        Player player) // The 'resource' is the full Player object
    {
        // Rule 1: Is the user a global Admin?
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
        {
            return; // Not authenticated
        }

        // Rule 2: Is the user updating their own linked player profile?
        if (player.UserId == currentUserId)
        {
            context.Succeed(requirement);
            return;
        }

        // Rule 3: Is the player unlinked, AND is the user a manager of that player's team?
        if (player.UserId == null)
        {
            var isTeamManager = await _context.TeamMemberships
                .AnyAsync(m => m.TeamId == player.TeamId &&
                               m.UserId == currentUserId &&
                               (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

            if (isTeamManager)
            {
                context.Succeed(requirement);
            }
        }
    }
}