using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.API.Services;

public class TopScorersService : ITopScorersService
{
    private readonly LeagueDbContext _context;

    public TopScorersService(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopScorerDto>> GetTopScorersAsync()
    {
        var topScorers = await _context.Goals
            .Where(g => g.Fixture != null && g.Fixture.Result != null && g.Fixture.Result.Status == ResultStatus.Approved)
            .GroupBy(g => new { g.PlayerId, g.Player!.Name, TeamName = g.Player.Team!.Name })
            .Select(group => new TopScorerDto
            {
                PlayerName = group.Key.Name,
                TeamName = group.Key.TeamName,
                Goals = group.Count()
            })
            .OrderByDescending(s => s.Goals)
            .ThenBy(s => s.PlayerName)
            .ToListAsync();

        return topScorers;
    }
}