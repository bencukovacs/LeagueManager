using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class PlayerService : IPlayerService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    public PlayerService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public async Task<PlayerResponseDto> CreatePlayerAsync(PlayerDto playerDto)
    {
        var teamExists = await _context.Teams.AnyAsync(t => t.Id == playerDto.TeamId);
        if (!teamExists)
        {
            throw new ArgumentException("Invalid Team ID.");
        }

        var player = _mapper.Map<Player>(playerDto);

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        // Reload with Team navigation property for mapping
        var createdPlayer = await _context.Players.Include(p => p.Team).FirstAsync(p => p.Id == player.Id);
        return _mapper.Map<PlayerResponseDto>(createdPlayer);
    }

    public async Task<PlayerResponseDto?> UpdatePlayerAsync(int id, PlayerDto playerDto)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return null;
        }

        _mapper.Map(playerDto, player);
        
        await _context.SaveChangesAsync();
        return _mapper.Map<PlayerResponseDto>(player);
    }

    public async Task<bool> DeletePlayerAsync(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return false;
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        return true;
    }
}
