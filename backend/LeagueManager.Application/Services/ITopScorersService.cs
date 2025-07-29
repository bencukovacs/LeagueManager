using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface ITopScorersService
{
    Task<IEnumerable<TopScorerDto>> GetTopScorersAsync();
}