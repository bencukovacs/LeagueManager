using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class PlayerService : IPlayerService
{
    private readonly LeagueDbContext _context;

    public PlayerService(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Player>> GetAllPlayersAsync()
    {
        return await _context.Players.Include(p => p.Team).ToListAsync();
    }

    public async Task<Player?> GetPlayerByIdAsync(int id)
    {
        return await _context.Players.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Player> CreatePlayerAsync(PlayerDto playerDto)
    {
        var teamExists = await _context.Teams.AnyAsync(t => t.Id == playerDto.TeamId);
        if (!teamExists)
        {
            throw new ArgumentException("Invalid Team ID.");
        }

        var player = new Player
        {
            Name = playerDto.Name,
            TeamId = playerDto.TeamId
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
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