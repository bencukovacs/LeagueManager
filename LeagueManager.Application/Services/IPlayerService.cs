using LeagueManager.Domain.Models;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface IPlayerService
{
    Task<IEnumerable<PlayerResponseDto>> GetAllPlayersAsync();
    Task<PlayerResponseDto?> GetPlayerByIdAsync(int id);
    Task<PlayerResponseDto> CreatePlayerAsync(PlayerDto playerDto);
    Task<PlayerResponseDto?> UpdatePlayerAsync(int id, PlayerDto playerDto);
    Task<bool> DeletePlayerAsync(int id);
}