using LeagueManager.API.Dtos;

namespace LeagueManager.API.Services;

public interface ITopScorersService
{
    Task<IEnumerable<TopScorerDto>> GetTopScorersAsync();
}