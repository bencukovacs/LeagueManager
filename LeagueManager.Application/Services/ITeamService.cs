using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;

namespace LeagueManager.Application.Services;

public interface ITeamService
{
    Task<IEnumerable<Team>> GetAllTeamsAsync();
    Task<Team?> GetTeamByIdAsync(int id);
    Task<Team> CreateTeamAsync(CreateTeamDto teamDto);
    Task UpdateTeamAsync(int id, CreateTeamDto teamDto);
    Task<bool> DeleteTeamAsync(int id);
}