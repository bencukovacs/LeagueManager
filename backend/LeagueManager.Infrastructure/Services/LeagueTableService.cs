using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class LeagueTableService : ILeagueTableService
{
    private readonly LeagueDbContext _context;

    public LeagueTableService(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeamStatsDto>> GetLeagueTableAsync()
    {
        // Only include approved teams in the league table
        var teams = await _context.Teams
            .Where(t => t.Status == TeamStatus.Approved)
            .ToListAsync();

        var approvedTeamIds = teams.Select(t => t.Id).ToHashSet();
        var approvedResults = await _context.Results
            .Where(r => r.Status == ResultStatus.Approved)
            .Include(r => r.Fixture)
            .ToListAsync();

        var stats = teams.Select(t => new TeamStatsDto { TeamName = t.Name }).ToList();

        foreach (var result in approvedResults)
        {
            // Skip any result that involves a team that is not approved
            var homeTeamId = result.Fixture!.HomeTeamId;
            var awayTeamId = result.Fixture!.AwayTeamId;
            if (!approvedTeamIds.Contains(homeTeamId) || !approvedTeamIds.Contains(awayTeamId))
            {
                continue;
            }

            var homeTeamName = teams.First(t => t.Id == homeTeamId).Name;
            var awayTeamName = teams.First(t => t.Id == awayTeamId).Name;
            var homeTeamStats = stats.First(s => s.TeamName == homeTeamName);
            var awayTeamStats = stats.First(s => s.TeamName == awayTeamName);

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
        
        return stats.OrderByDescending(s => s.Points)
                    .ThenByDescending(s => s.GoalDifference)
                    .ThenByDescending(s => s.GoalsFor);
    }
}