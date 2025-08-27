using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILeagueConfigurationService _configService;

  public TeamService(LeagueDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILeagueConfigurationService configService)
  {
    _context = context;
    _mapper = mapper;
    _httpContextAccessor = httpContextAccessor;
    _configService = configService;
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
        var nameExists = await _context.Teams.AnyAsync(t => t.Name.ToLower() == teamDto.Name.ToLower());
        if (nameExists)
        {
            throw new InvalidOperationException("A team with this name already exists.");
        }
        
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }

        if (!currentUser.IsInRole("Admin"))
        {
            // Rule 1: Check if a non-admin user already manages a team.
            var userAlreadyManagesTeam = await _context.TeamMemberships
                .AnyAsync(m => m.UserId == currentUserId && (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

            if (userAlreadyManagesTeam)
            {
                // If they do, throw an exception to prevent them from creating another one.
                throw new InvalidOperationException("You already manage a team and cannot create another one.");
            }
            
            // Rule 2: Check if the user has a pending request to join another team.
            var hasPendingJoinRequest = await _context.RosterRequests
                .AnyAsync(r => r.UserId == currentUserId && r.Status == RosterRequestStatus.PendingLeaderApproval);
            if (hasPendingJoinRequest)
            {
                throw new InvalidOperationException("You cannot create a team while you have a pending request to join another.");
            }
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

        var playerProfile = await _context.Players.FirstOrDefaultAsync(p => p.UserId == currentUserId);
        if (playerProfile != null)
        {
            playerProfile.TeamId = team.Id;
        }

        await _context.SaveChangesAsync();

        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<TeamResponseDto> CreateTeamAsAdminAsync(CreateTeamDto teamDto)
    {
        var nameExists = await _context.Teams.AnyAsync(t => t.Name.ToLower() == teamDto.Name.ToLower());
        if (nameExists)
        {
            throw new InvalidOperationException("A team with this name already exists.");
        }
        
        var team = _mapper.Map<Team>(teamDto);

        // Admins create teams that are instantly approved
        team.Status = TeamStatus.Approved;

        _context.Teams.Add(team);
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

        var config = await _configService.GetConfigurationAsync();
        var minPlayersRequired = config.MinPlayersPerTeam;

        var playerCount = await _context.Players.CountAsync(p => p.TeamId == teamId);

        if (playerCount < minPlayersRequired)
        {
            throw new InvalidOperationException($"Team cannot be approved. It has {playerCount} players but requires at least {minPlayersRequired}.");
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

    public async Task<MyTeamAndConfigResponseDto> GetMyTeamAndConfigAsync()
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        MyTeamResponseDto? myTeam = null;
        if (!string.IsNullOrEmpty(currentUserId))
        {
            var membership = await _context.TeamMemberships
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.UserId == currentUserId);

            if (membership != null && membership.Team != null)
            {
                myTeam = new MyTeamResponseDto
                {
                    Team = _mapper.Map<TeamResponseDto>(membership.Team),
                    UserRole = membership.Role.ToString()
                };
            }
        }

        var config = await _configService.GetConfigurationAsync();

        return new MyTeamAndConfigResponseDto
        {
            MyTeam = myTeam,
            Config = config
        };
    }
    
    public async Task<IEnumerable<FixtureResponseDto>> GetFixturesForMyTeamAsync()
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Enumerable.Empty<FixtureResponseDto>();
        }

        // Find the team ID for the current user
        var teamId = await _context.TeamMemberships
            .Where(m => m.UserId == currentUserId)
            .Select(m => (int?)m.TeamId) // Cast to nullable int to handle no-team case
            .FirstOrDefaultAsync();

        if (teamId == null)
        {
            return Enumerable.Empty<FixtureResponseDto>();
        }

        // Fetch all fixtures where the team is either home or away
        var fixtures = await _context.Fixtures
            .Where(f => f.HomeTeamId == teamId.Value || f.AwayTeamId == teamId.Value)
            .OrderByDescending(f => f.KickOffDateTime) // Show most recent first
            .ProjectTo<FixtureResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return fixtures;
    }
    
    public async Task LeaveMyTeamAsync()
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var membership = await _context.TeamMemberships
            .Include(m => m.Team) // We need the team's status
            .FirstOrDefaultAsync(m => m.UserId == currentUserId);

        if (membership == null || membership.Team == null)
        {
            throw new InvalidOperationException("User is not a member of any team.");
        }

        if (membership.Team.Status == TeamStatus.PendingApproval)
        {
            // Any member leaving a pending team triggers a full cancellation if they are the leader.
            if (membership.Role == TeamRole.Leader)
            {
                var roster = await _context.Players
                    .Where(p => p.TeamId == membership.TeamId)
                    .ToListAsync();

                foreach (var player in roster)
                {
                    if (player.UserId == null) _context.Players.Remove(player);
                    else player.TeamId = null;
                }
                _context.Teams.Remove(membership.Team);
            }
            else
            {
                _context.TeamMemberships.Remove(membership);
            }
        }
        // Scenario 2: The team is already approved
        else
        {
            // Business Rule: The last leader cannot leave an approved team.
            if (membership.Role == TeamRole.Leader)
            {
                var otherManagers = await _context.TeamMemberships.AnyAsync(m =>
                    m.TeamId == membership.TeamId &&
                    m.UserId != currentUserId &&
                    m.Role == TeamRole.Leader);

                if (!otherManagers)
                {
                    throw new InvalidOperationException("You are the last leader of this team. You must transfer leadership before leaving.");
                }
            }
            // For any role (Member, Assistant, or a Leader who is NOT the last one), just remove the membership.
            // The associated player will be detached in the next step.
            _context.TeamMemberships.Remove(membership);
        }

        // Detach the player profile for any user leaving an approved team
        var playerProfile = await _context.Players.FirstOrDefaultAsync(p => p.UserId == currentUserId);
        if (playerProfile != null)
        {
            playerProfile.TeamId = null;
        }

        await _context.SaveChangesAsync();
    }
    
    public async Task DisbandMyTeamAsync()
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var membership = await _context.TeamMemberships
            .Include(m => m.Team) // We need the team's status
            .FirstOrDefaultAsync(m => m.UserId == currentUserId);

        if (membership?.Role != TeamRole.Leader || membership.Team?.Status != TeamStatus.Approved)
        {
            throw new UnauthorizedAccessException("Only the leader of an approved team can disband it.");
        }

        var team = membership.Team;
        var roster = await _context.Players.Where(p => p.TeamId == team.Id).ToListAsync();
        var allMemberships = await _context.TeamMemberships.Where(m => m.TeamId == team.Id).ToListAsync();

        // Soft-delete all players
        foreach (var player in roster)
        {
            player.TeamId = null;
        }

        // Delete all memberships
        _context.TeamMemberships.RemoveRange(allMemberships);
        // Delete the team
        _context.Teams.Remove(team);

        await _context.SaveChangesAsync();
    }
}