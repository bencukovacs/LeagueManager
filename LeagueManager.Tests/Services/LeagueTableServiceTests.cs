using LeagueManager.API.Data;
using LeagueManager.API.Models;
using LeagueManager.API.Services;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Tests.Services;

public class LeagueTableServiceTests
{
    // Helper method to create a fresh in-memory database for each test
    private LeagueDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<LeagueDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique name to ensure tests are isolated
            .Options;
        var context = new LeagueDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetLeagueTableAsync_WithApprovedResults_CalculatesPointsCorrectly()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();

        var teamA = new Team { Id = 1, Name = "Team A" };
        var teamB = new Team { Id = 2, Name = "Team B" };
        var teamC = new Team { Id = 3, Name = "Team C" };
        context.Teams.AddRange(teamA, teamB, teamC);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 }; // A vs B
        var fixture2 = new Fixture { Id = 2, HomeTeamId = 1, AwayTeamId = 3 }; // A vs C
        context.Fixtures.AddRange(fixture1, fixture2);
        
        // Result 1: Team A (3) - Team B (1) -> Team A wins
        var result1 = new Result { Id = 1, FixtureId = 1, HomeScore = 3, AwayScore = 1, Status = ResultStatus.Approved };
        // Result 2: Team A (2) - Team C (2) -> Draw
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

        // Check Team A: 1 Win, 1 Draw = 4 points
        Assert.Equal(4, teamAStats.Points);
        Assert.Equal(2, teamAStats.Played);
        Assert.Equal(1, teamAStats.Won);
        Assert.Equal(1, teamAStats.Drawn);

        // Check Team B: 1 Loss = 0 points
        Assert.Equal(0, teamBStats.Points);
        Assert.Equal(1, teamBStats.Played);
        Assert.Equal(1, teamBStats.Lost);

        // Check Team C: 1 Draw = 1 point
        Assert.Equal(1, teamCStats.Points);
        Assert.Equal(1, teamCStats.Played);
        Assert.Equal(1, teamCStats.Drawn);
    }

    [Fact]
    public async Task GetLeagueTableAsync_WithNoApprovedResults_ReturnsEmptyStats()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();
        
        var teamA = new Team { Id = 1, Name = "Team A" };
        context.Teams.Add(teamA);
        await context.SaveChangesAsync();
        
        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        Assert.Single(table);
        var stats = table.First();
        Assert.Equal("Team A", stats.TeamName);
        Assert.Equal(0, stats.Played);
        Assert.Equal(0, stats.Points);
    }
    
    [Fact]
    public async Task GetLeagueTableAsync_IgnoresPendingAndDisputedResults()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();
        
        var teamA = new Team { Id = 1, Name = "Team A" };
        var teamB = new Team { Id = 2, Name = "Team B" };
        context.Teams.AddRange(teamA, teamB);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        var fixture2 = new Fixture { Id = 2, HomeTeamId = 1, AwayTeamId = 2 };
        context.Fixtures.AddRange(fixture1, fixture2);
        
        var pendingResult = new Result { FixtureId = 1, HomeScore = 1, AwayScore = 0, Status = ResultStatus.PendingApproval };
        var disputedResult = new Result { FixtureId = 2, HomeScore = 1, AwayScore = 0, Status = ResultStatus.Disputed };
        context.Results.AddRange(pendingResult, disputedResult);
        
        await context.SaveChangesAsync();

        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        var teamAStats = table.First(t => t.TeamName == "Team A");
        Assert.Equal(0, teamAStats.Played); // Should not have counted the pending/disputed games
        Assert.Equal(0, teamAStats.Points);
    }

    [Fact]
    public async Task GetLeagueTableAsync_SortsByGoalDifferenceWhenPointsAreEqual()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();
        
        var teamA = new Team { Id = 1, Name = "Team A" }; // GD = +1 (2-1)
        var teamB = new Team { Id = 2, Name = "Team B" }; // GD = +2 (3-1)
        context.Teams.AddRange(teamA, teamB);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        context.Fixtures.Add(fixture1);
        
        // Both teams have 3 points
        var result1 = new Result { FixtureId = 1, HomeScore = 2, AwayScore = 3, Status = ResultStatus.Approved }; // B wins
        context.Results.Add(result1);
        
        await context.SaveChangesAsync();
        
        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        Assert.Equal(2, table.Count);
        // Team B should be first because of better Goal Difference
        Assert.Equal("Team B", table[0].TeamName);
        Assert.Equal("Team A", table[1].TeamName);
    }

    [Fact]
    public async Task GetLeagueTableAsync_SortsByGoalsForWhenPointsAndGDAreEqual()
    {
        // --- ARRANGE ---
        await using var context = GetDbContext();
        
        var teamA = new Team { Id = 1, Name = "Team A" }; // GF = 2 (2-1)
        var teamB = new Team { Id = 2, Name = "Team B" }; // GF = 3 (3-2)
        context.Teams.AddRange(teamA, teamB);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        context.Fixtures.Add(fixture1);
        
        // Both have 3 points and +1 GD
        var result1 = new Result { FixtureId = 1, HomeScore = 2, AwayScore = 3, Status = ResultStatus.Approved }; // B wins 3-2
        context.Results.Add(result1);
        
        await context.SaveChangesAsync();
        
        var service = new LeagueTableService(context);

        // --- ACT ---
        var table = (await service.GetLeagueTableAsync()).ToList();

        // --- ASSERT ---
        Assert.Equal(2, table.Count);
        // Team B should be first because of more Goals For
        Assert.Equal("Team B", table[0].TeamName);
        Assert.Equal("Team A", table[1].TeamName);
    }
}