using LeagueManager.Domain.Models;
using LeagueManager.Application.Dtos;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeagueManager.API.Authorization;

public class CanSubmitResultHandler : AuthorizationHandler<CanSubmitResultRequirement, FixtureResponseDto>
{
    private readonly LeagueDbContext _context;

    public CanSubmitResultHandler(LeagueDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanSubmitResultRequirement requirement,
        FixtureResponseDto fixture) // The resource is the Fixture DTO passed by the controller
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
            .AnyAsync(m => (m.TeamId == fixture.HomeTeam.Id || m.TeamId == fixture.AwayTeam.Id) &&
                           m.UserId == userId &&
                           (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        if (isTeamManager)
        {
            context.Succeed(requirement);
        }
    }
}