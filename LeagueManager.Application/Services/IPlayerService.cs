using LeagueManager.Domain.Models;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface IPlayerService
{
    Task<IEnumerable<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerByIdAsync(int id);
    Task<Player> CreatePlayerAsync(PlayerDto playerDto);
    Task<bool> DeletePlayerAsync(int id);
}