using LeagueManager.API.Dtos;

namespace LeagueManager.API.Services;

public interface ILeagueTableService
{
    Task<IEnumerable<TeamStatsDto>> GetLeagueTableAsync();
}