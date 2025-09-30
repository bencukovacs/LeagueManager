using LeagueManager.Infrastructure.Data;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace LeagueManager.Tests.Services;

public class LeagueTableServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private bool _disposed;

    public LeagueTableServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<LeagueDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new LeagueDbContext(_options);
        context.Database.EnsureCreated();
    }

    private LeagueDbContext GetDbContext() => new LeagueDbContext(_options);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection.Close();
                _connection.Dispose();
            }
            _disposed = true;
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetLeagueTableAsync_WithApprovedResults_CalculatesPointsCorrectly()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();

        var teamA = new Team { Id = 1, Name = "Team A", Status = TeamStatus.Approved };
        var teamB = new Team { Id = 2, Name = "Team B", Status = TeamStatus.Approved };
        var teamC = new Team { Id = 3, Name = "Team C", Status = TeamStatus.Approved };
        context.Teams.AddRange(teamA, teamB, teamC);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        var fixture2 = new Fixture { Id = 2, HomeTeamId = 1, AwayTeamId = 3 };
        context.Fixtures.AddRange(fixture1, fixture2);
        
        var result1 = new Result { Id = 1, FixtureId = 1, HomeScore = 3, AwayScore = 1, Status = ResultStatus.Approved };
        var result2 = new Result { Id = 2, FixtureId = 2, HomeScore = 2, AwayScore = 2, Status = ResultStatus.Approved };
        context.Results.AddRange(result1, result2);
        
        await context.SaveChangesAsync();

        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        var teamAStats = table.First(t => t.TeamName == "Team A");
        var teamBStats = table.First(t => t.TeamName == "Team B");
        var teamCStats = table.First(t => t.TeamName == "Team C");

        Assert.Equal(4, teamAStats.Points);
        Assert.Equal(0, teamBStats.Points);
        Assert.Equal(1, teamCStats.Points);
    }

    [Fact]
    public async Task GetLeagueTableAsync_WithNoApprovedResults_ReturnsEmptyStats()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();
        
        var teamA = new Team { Id = 1, Name = "Team A", Status = TeamStatus.Approved };
        context.Teams.Add(teamA);
        await context.SaveChangesAsync();
        
        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        Assert.Single(table);
        var stats = table[0];
        Assert.Equal(0, stats.Played);
        Assert.Equal(0, stats.Points);
    }
    
    [Fact]
    public async Task GetLeagueTableAsync_IgnoresPendingAndDisputedResults()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();
        
        var teamA = new Team { Id = 1, Name = "Team A", Status = TeamStatus.Approved };
        var teamB = new Team { Id = 2, Name = "Team B", Status = TeamStatus.Approved };
        context.Teams.AddRange(teamA, teamB);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        context.Fixtures.Add(fixture1);
        
        var pendingResult = new Result { FixtureId = 1, HomeScore = 1, AwayScore = 0, Status = ResultStatus.PendingApproval };
        context.Results.Add(pendingResult);
        
        await context.SaveChangesAsync();

        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        var teamAStats = table.First(t => t.TeamName == "Team A");
        Assert.Equal(0, teamAStats.Played);
        Assert.Equal(0, teamAStats.Points);
    }

    [Fact]
    public async Task GetLeagueTableAsync_SortsByGoalDifferenceWhenPointsAreEqual()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();
        
        var teamA = new Team { Id = 1, Name = "Team A", Status = TeamStatus.Approved }; // GD = +1 (2-1 win)
        var teamB = new Team { Id = 2, Name = "Team B", Status = TeamStatus.Approved }; // GD = +2 (3-1 win)
        var teamC = new Team { Id = 3, Name = "Team C", Status = TeamStatus.Approved };
        context.Teams.AddRange(teamA, teamB, teamC);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 3 }; // A vs C
        var fixture2 = new Fixture { Id = 2, HomeTeamId = 2, AwayTeamId = 3 }; // B vs C
        context.Fixtures.AddRange(fixture1, fixture2);
        
        var result1 = new Result { FixtureId = 1, HomeScore = 2, AwayScore = 1, Status = ResultStatus.Approved }; // A wins
        var result2 = new Result { FixtureId = 2, HomeScore = 3, AwayScore = 1, Status = ResultStatus.Approved }; // B wins
        context.Results.AddRange(result1, result2);
        
        await context.SaveChangesAsync();
        
        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        Assert.Equal("Team B", table[0].TeamName); // Team B should be first with better GD
        Assert.Equal("Team A", table[1].TeamName);
    }
}