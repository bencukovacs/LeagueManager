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

        // This action only removes the player from a team.
        player.TeamId = null;
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
        return await _context.Players
            .Where(p => p.TeamId == teamId)
            .ProjectTo<PlayerResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
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
