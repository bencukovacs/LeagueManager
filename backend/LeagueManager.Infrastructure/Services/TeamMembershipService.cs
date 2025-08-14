using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeagueManager.Infrastructure.Services;

public class TeamMembershipService : ITeamMembershipService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TeamMembershipService(LeagueDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    // --- THIS IS THE FINAL, CORRECTED METHOD ---
    public async Task<IEnumerable<TeamMemberResponseDto>> GetMembersForTeamAsync(int teamId)
    {
        // 1. Fetch the raw data with the necessary includes.
        var memberships = await _context.TeamMemberships
            .Where(m => m.TeamId == teamId)
            .Include(m => m.User) 
            .ToListAsync();
        
        // 2. Manually project the results into the DTO. This is the most reliable way.
        return memberships.Select(m => new TeamMemberResponseDto
        {
            UserId = m.UserId,
            Email = m.User?.Email ?? string.Empty,
            FullName = m.User?.FullName ?? string.Empty,
            Role = m.Role.ToString()
        });
    }

    public async Task<TeamMemberResponseDto?> UpdateMemberRoleAsync(int teamId, string memberUserId, UpdateTeamMemberRoleDto dto)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var actorMembership = await _context.TeamMemberships
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == currentUserId);

        if (actorMembership?.Role != TeamRole.Leader)
        {
            throw new UnauthorizedAccessException("Only the Team Leader can change member roles.");
        }
        
        var targetMembership = await _context.TeamMemberships
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == memberUserId);

        if (targetMembership == null)
        {
            return null;
        }

        if (dto.NewRole == TeamRole.AssistantLeader)
        {
            var hasAssistant = await _context.TeamMemberships
                .AnyAsync(m => m.TeamId == teamId && m.Role == TeamRole.AssistantLeader && m.UserId != memberUserId);
            if (hasAssistant)
            {
                throw new InvalidOperationException("This team already has an Assistant Leader.");
            }
        }

        if (dto.NewRole == TeamRole.Leader)
        {
            if (targetMembership.Role != TeamRole.AssistantLeader)
            {
                throw new InvalidOperationException("Leadership can only be transferred to an Assistant Leader.");
            }
            actorMembership.Role = TeamRole.AssistantLeader;
        }

        targetMembership.Role = dto.NewRole;
        await _context.SaveChangesAsync();

        return _mapper.Map<TeamMemberResponseDto>(targetMembership);
    }
}