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

public class RosterRequestService : IRosterRequestService
{
    private const string NotAuthedMsg = "User is not authenticated.";
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RosterRequestService(LeagueDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RosterRequestResponseDto> CreateJoinRequestAsync(int teamId)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var team = await _context.Teams.FindAsync(teamId)
                   ?? throw new ArgumentException("Team not found.");
            
        // Rule 1: Check if the user already has a pending request for ANY team.
        var existingRequest = await _context.RosterRequests.AnyAsync(r => 
            r.UserId == currentUserId &&
            r.Status == RosterRequestStatus.PendingLeaderApproval);
        if (existingRequest)
        {
            throw new InvalidOperationException("You already have a pending request to join a team and cannot send another.");
        }

        // Rule 2: Check if the user is the leader of a different PENDING team.
        var managesPendingTeam = await _context.TeamMemberships
            .AnyAsync(m => m.UserId == currentUserId && 
                           m.Role == TeamRole.Leader &&
                           m.Team != null &&
                           m.Team.Status == TeamStatus.PendingApproval);
        if (managesPendingTeam)
        {
            throw new InvalidOperationException("You cannot join a team while your own team application is pending approval.");
        }

        var request = new RosterRequest
        {
            UserId = currentUserId,
            TeamId = teamId,
            Type = RosterRequestType.JoinRequest,
            Status = RosterRequestStatus.PendingLeaderApproval
        };

        _context.RosterRequests.Add(request);
        await _context.SaveChangesAsync();

        return _mapper.Map<RosterRequestResponseDto>(request);
    }

    public async Task<IEnumerable<RosterRequestResponseDto>> GetPendingJoinRequestsForMyTeamAsync()
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(NotAuthedMsg);

        var teamMembership = await _context.TeamMemberships.FirstOrDefaultAsync(m => m.UserId == currentUserId && (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));
        if (teamMembership == null)
        {
            return Enumerable.Empty<RosterRequestResponseDto>();
        }

        return await _context.RosterRequests
            .Where(r => r.TeamId == teamMembership.TeamId && r.Status == RosterRequestStatus.PendingLeaderApproval)
            .ProjectTo<RosterRequestResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TeamMembership> ApproveJoinRequestAsync(int requestId)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(NotAuthedMsg);

        var request = await _context.RosterRequests.FindAsync(requestId)
            ?? throw new ArgumentException("Request not found.");

        // Authorization check: Is the current user a manager of the team in the request?
        var isManager = await _context.TeamMemberships.AnyAsync(m =>
            m.TeamId == request.TeamId && m.UserId == currentUserId &&
            (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        if (!isManager)
        {
            throw new UnauthorizedAccessException("You are not authorized to manage this team's requests.");
        }

        // Find the player profile linked to the user who made the request
        var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == request.UserId)
            ?? throw new InvalidOperationException("Player profile for the requesting user not found.");

        // Business Rule: A player can only be on one team at a time.
        if (player.TeamId.HasValue)
        {
            throw new InvalidOperationException("This player is already on a team.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // 1. Update the player's TeamId
        player.TeamId = request.TeamId;

        // 2. Create the new TeamMembership record
        var newMembership = new TeamMembership
        {
            UserId = request.UserId,
            TeamId = request.TeamId,
            Role = TeamRole.Member // New members are always assigned the "Member" role
        };
        _context.TeamMemberships.Add(newMembership);

        // 3. Update the request status
        request.Status = RosterRequestStatus.Approved;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return newMembership;
    }

    public async Task RejectJoinRequestAsync(int requestId)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(NotAuthedMsg);

        var request = await _context.RosterRequests.FindAsync(requestId)
            ?? throw new ArgumentException("Request not found.");

        var isManager = await _context.TeamMemberships.AnyAsync(m =>
            m.TeamId == request.TeamId && m.UserId == currentUserId &&
            (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        if (!isManager)
        {
            throw new UnauthorizedAccessException("You are not authorized to manage this team's requests.");
        }

        request.Status = RosterRequestStatus.Rejected;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RosterRequestResponseDto>> GetMyPendingRequestsAsync()
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(NotAuthedMsg);

        return await _context.RosterRequests
            .Where(r => r.UserId == currentUserId &&
                        (r.Status == RosterRequestStatus.PendingLeaderApproval || r.Status == RosterRequestStatus.PendingPlayerAcceptance))
            .ProjectTo<RosterRequestResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task CancelMyRequestAsync(int requestId)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(NotAuthedMsg);

        var request = await _context.RosterRequests.FindAsync(requestId)
            ?? throw new ArgumentException("Request not found.");

        // Authorization check: Does this request belong to the current user?
        if (request.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to cancel this request.");
        }

        // Business Rule: You can only cancel pending requests.
        if (request.Status != RosterRequestStatus.PendingLeaderApproval && request.Status != RosterRequestStatus.PendingPlayerAcceptance)
        {
            throw new InvalidOperationException("This request cannot be cancelled as it is already resolved.");
        }

        _context.RosterRequests.Remove(request);
        await _context.SaveChangesAsync();
    }
}