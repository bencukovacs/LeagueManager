using LeagueManager.Data;
using LeagueManager.Dtos;
using LeagueManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Services;

public class LeagueTableService : ILeagueTableService
{
    private readonly LeagueDbContext _context;

    public LeagueTableService(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeamStatsDto>> GetLeagueTableAsync()
    {
        var teams = await _context.Teams.ToListAsync();
        
        var approvedResults = await _context.Results
            .Where(r => r.Status == ResultStatus.Approved)
            .Include(r => r.Fixture)
            .ToListAsync();

        var statsList = teams.Select(t => new TeamStatsDto { TeamName = t.Name }).ToList();

        foreach (var result in approvedResults)
        {
            if (result.Fixture == null) continue;

            var homeTeamStats = statsList.FirstOrDefault(s => s.TeamName == teams.First(t => t.Id == result.Fixture.HomeTeamId).Name);
            var awayTeamStats = statsList.FirstOrDefault(s => s.TeamName == teams.First(t => t.Id == result.Fixture.AwayTeamId).Name);

            if (homeTeamStats == null || awayTeamStats == null) continue;

            homeTeamStats.Played++;
            awayTeamStats.Played++;
            homeTeamStats.GoalsFor += result.HomeScore;
            awayTeamStats.GoalsFor += result.AwayScore;
            homeTeamStats.GoalsAgainst += result.AwayScore;
            awayTeamStats.GoalsAgainst += result.HomeScore;

            if (result.HomeScore > result.AwayScore)
            {
                homeTeamStats.Won++;
                homeTeamStats.Points += 3;
                awayTeamStats.Lost++;
            }
            else if (result.AwayScore > result.HomeScore) 
            {
                awayTeamStats.Won++;
                awayTeamStats.Points += 3;
                homeTeamStats.Lost++;
            }
            else
            {
                homeTeamStats.Drawn++;
                homeTeamStats.Points += 1;
                awayTeamStats.Drawn++;
                awayTeamStats.Points += 1;
            }
        }
        
        return statsList.OrderByDescending(s => s.Points)
                        .ThenByDescending(s => s.GoalDifference)
                        .ThenByDescending(s => s.GoalsFor);
    }
}