using LeagueManager.Dtos;

namespace LeagueManager.Services;

public interface ILeagueTableService
{
    Task<IEnumerable<TeamStatsDto>> GetLeagueTableAsync();
}