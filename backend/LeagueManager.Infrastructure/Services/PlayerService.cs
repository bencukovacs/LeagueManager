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

public class PlayerService : IPlayerService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public PlayerService(LeagueDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<PlayerResponseDto>> GetAllPlayersAsync()
    {
        var players = await _context.Players.Include(p => p.Team).ToListAsync();
        return _mapper.Map<IEnumerable<PlayerResponseDto>>(players);
    }

    public async Task<PlayerResponseDto?> GetPlayerByIdAsync(int id)
    {
        var player = await _context.Players.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == id);
        if (player == null) return null;

        return _mapper.Map<PlayerResponseDto>(player);
    }

    public async Task<PlayerResponseDto?> UpdatePlayerAsync(int id, PlayerDto playerDto)
    {
        var player = await _context.Players
            .Include(p => p.Team)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
        {
            return null;
        }

        player.Name = playerDto.Name;

        await _context.SaveChangesAsync();

        return _mapper.Map<PlayerResponseDto>(player);
    }

    public async Task<PlayerResponseDto> CreatePlayerAsync(PlayerDto playerDto)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("User context is not available.");
        var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        _ = await _context.Teams.FindAsync(playerDto.TeamId)
            ?? throw new ArgumentException("Invalid Team ID.");

        var isManager = await _context.TeamMemberships.AnyAsync(m =>
            m.TeamId == playerDto.TeamId && m.UserId == currentUserId &&
            (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        var isAdmin = currentUser.IsInRole("Admin");

        var player = _mapper.Map<Player>(playerDto);

        // SCENARIO 1: A manager or admin is adding a player to a roster.
        if (isManager || isAdmin)
        {
            // The player's UserId should be null. This is correct.
        }
        // SCENARIO 2: A regular user is creating their own player profile.
        else
        {
            // Business Rule: A user can only have one player profile.
            var userAlreadyHasPlayerProfile = await _context.Players.AnyAsync(p => p.UserId == currentUserId);
            if (userAlreadyHasPlayerProfile)
            {
                throw new InvalidOperationException("This user account is already linked to a player profile.");
            }
            // Link this new player to the current user's account.
            player.UserId = currentUserId;
        }

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return await GetPlayerByIdAsync(player.Id)
               ?? throw new InvalidOperationException("Could not retrieve created player.");
    }

    public async Task RemovePlayerFromRosterAsync(int id)
    {
        var player = await _context.Players.FindAsync(id)
                     ?? throw new KeyNotFoundException("Player not found.");

        var currentUser = _httpContextAccessor.HttpContext?.User 
                          ?? throw new UnauthorizedAccessException("User context is not available.");
            
        var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

        var isTeamManager = await _context.TeamMemberships
            .AnyAsync(m => m.TeamId == player.TeamId && m.UserId == currentUserId &&
                           (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        if (!currentUser.IsInRole("Admin") && !isTeamManager)
        {
            throw new UnauthorizedAccessException("User is not authorized to remove this player.");
        }

        // Capture the original TeamId before we change it.
        var originalTeamId = player.TeamId;
        player.TeamId = null;

        if (player.UserId != null)
        {
            // Use the originalTeamId to find the correct membership record.
            var membership = await _context.TeamMemberships
                .FirstOrDefaultAsync(m => m.UserId == player.UserId && m.TeamId == originalTeamId);

            if (membership != null)
            {
                _context.TeamMemberships.Remove(membership);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeletePlayerPermanentlyAsync(int id)
    {
        var player = await _context.Players.FindAsync(id)
            ?? throw new KeyNotFoundException("Player not found.");

        // This action permanently removes the player.
        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
    }

    public async Task<Player?> GetDomainPlayerByIdAsync(int id)
    {
        // This method fetches the full domain object with all relationships
        // needed for our complex authorization checks.
        return await _context.Players
            .Include(p => p.Team)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PlayerResponseDto>> GetPlayersForTeamAsync(int teamId)
    {
        // This query now joins Players with TeamMemberships to get the role for each player.
        // This allows us to have a single, unified roster view on the frontend.
        var players = await _context.Players
            .Where(p => p.TeamId == teamId)
            .GroupJoin( // This performs a LEFT JOIN in memory
                _context.TeamMemberships,
                player => player.UserId,
                membership => membership.UserId,
                (player, memberships) => new { player, membership = memberships.FirstOrDefault() }
            )
            .Select(x => new PlayerResponseDto
            {
                Id = x.player.Id,
                Name = x.player.Name,
                TeamId = x.player.TeamId,
                TeamName = x.player.Team != null ? x.player.Team.Name : string.Empty,
                UserId = x.player.UserId,
                UserRole = x.membership != null ? x.membership.Role.ToString() : null
            })
            .ToListAsync();

        return players;
    }

    public async Task<IEnumerable<PlayerResponseDto>> GetUnassignedPlayersAsync()
    {
        return await _context.Players
            .Where(p => p.TeamId == null)
            .ProjectTo<PlayerResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
    
    public async Task<PlayerResponseDto?> AssignPlayerToTeamAsync(int playerId, int teamId)
    {
        var player = await _context.Players.FindAsync(playerId);
        var teamExists = await _context.Teams.AnyAsync(t => t.Id == teamId && t.Status == TeamStatus.Approved);

        if (player == null || !teamExists)
        {
            return null; // Or throw an exception if you prefer
        }

        player.TeamId = teamId;
        await _context.SaveChangesAsync();

        return await GetPlayerByIdAsync(playerId);
    }
}
