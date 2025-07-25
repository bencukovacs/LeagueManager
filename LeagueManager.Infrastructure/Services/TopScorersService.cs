using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class TopScorersService : ITopScorersService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper; 

    public TopScorersService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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