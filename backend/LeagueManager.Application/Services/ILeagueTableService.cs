using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface ILeagueTableService
{
    Task<IEnumerable<TeamStatsDto>> GetLeagueTableAsync();
}