using AutoMapper;
using AutoMapper.QueryableExtensions;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeagueManager.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TeamService(LeagueDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync()
    {
        return await _context.Teams
            .Where(t => t.Status == TeamStatus.Approved)
            .ProjectTo<TeamResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IEnumerable<TeamResponseDto>> GetAllTeamsForAdminAsync()
    {
        return await _context.Teams
            .ProjectTo<TeamResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TeamResponseDto?> GetTeamByIdAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<TeamResponseDto> CreateTeamAsync(CreateTeamDto teamDto)
    {
        // Get the ID of the currently logged-in user
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var team = _mapper.Map<Team>(teamDto);
        team.Status = TeamStatus.PendingApproval;

        _context.Teams.Add(team);

        // We must save here first to get the new team's ID
        await _context.SaveChangesAsync();

        // Create the TeamMembership record, making the creator the "Leader"
        var membership = new TeamMembership
        {
            UserId = currentUserId,
            TeamId = team.Id,
            Role = TeamRole.Leader
        };
        _context.TeamMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<TeamResponseDto?> ApproveTeamAsync(int teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            return null;
        }

        const int MinPlayersRequired = 6;
        var playerCount = await _context.Players.CountAsync(p => p.TeamId == teamId);

        if (playerCount < MinPlayersRequired)
        {
            throw new InvalidOperationException($"Team cannot be approved. It has {playerCount} players but requires at least {MinPlayersRequired}.");
        }

        if (string.IsNullOrWhiteSpace(team.PrimaryColor))
        {
            throw new InvalidOperationException("Team cannot be approved. A primary color must be set.");
        }

        team.Status = TeamStatus.Approved;
        await _context.SaveChangesAsync();

        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<TeamResponseDto?> UpdateTeamAsync(int id, CreateTeamDto teamDto)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return null;
        }
        _mapper.Map(teamDto, team);
        await _context.SaveChangesAsync();
        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return false;
        }
        var isUsed = await _context.Fixtures.AnyAsync(f => f.HomeTeamId == id || f.AwayTeamId == id);
        if (isUsed)
        {
            throw new InvalidOperationException("Cannot delete a team that is currently assigned to a fixture.");
        }
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<TeamResponseDto>> GetPendingTeamsAsync()
    {
        return await _context.Teams
            .Where(t => t.Status == TeamStatus.PendingApproval)
            .ProjectTo<TeamResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}