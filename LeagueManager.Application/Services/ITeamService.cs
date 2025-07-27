using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;

namespace LeagueManager.Application.Services;

public interface ITeamService
{
    Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync();
    Task<TeamResponseDto?> GetTeamByIdAsync(int id);
    Task<TeamResponseDto> CreateTeamAsync(CreateTeamDto teamDto);
    Task<TeamResponseDto?> UpdateTeamAsync(int id, CreateTeamDto teamDto);
    Task<bool> DeleteTeamAsync(int id);
    Task<TeamResponseDto?> ApproveTeamAsync(int teamId);
    Task<IEnumerable<TeamResponseDto>> GetPendingTeamsAsync();
}