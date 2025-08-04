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
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var canManageTeam = await _context.TeamMemberships
            .AnyAsync(m => m.TeamId == playerDto.TeamId && m.UserId == currentUserId &&
                           (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        var userAlreadyHasPlayerProfile = await _context.Players.AnyAsync(p => p.UserId == currentUserId);

        // Authorization check: Allow if they are a manager OR if they don't have a profile yet.
        if (!canManageTeam && userAlreadyHasPlayerProfile)
        {
            throw new UnauthorizedAccessException("User is not authorized to create a player for this team or already has a player profile.");
        }

        var team = await _context.Teams.FindAsync(playerDto.TeamId)
            ?? throw new ArgumentException("Invalid Team ID.");

        var player = _mapper.Map<Player>(playerDto);

        // If the user is creating their own profile, link it to their user account.
        if (!canManageTeam)
        {
            player.UserId = currentUserId;
        }

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return await GetPlayerByIdAsync(player.Id)
               ?? throw new InvalidOperationException("Could not retrieve created player.");
    }

    // This method now contains the complex deletion logic
    public async Task DeletePlayerAsync(int id)
    {
        var player = await _context.Players.FindAsync(id)
            ?? throw new KeyNotFoundException("Player not found.");

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserIsAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

        if (currentUserIsAdmin)
        {
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return;
        }

        var isTeamManager = await _context.TeamMemberships
            .AnyAsync(m => m.TeamId == player.TeamId && m.UserId == currentUserId &&
                           (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        // Allow deletion only if the player is unlinked AND the user is a team manager.
        if (player.UserId == null && isTeamManager)
        {
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return;
        }

        // If neither of the above conditions are met, the user is not authorized.
        throw new UnauthorizedAccessException("User is not authorized to delete this player.");
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
}
