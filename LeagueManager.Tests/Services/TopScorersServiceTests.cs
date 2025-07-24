using LeagueManager.Infrastructure.Data;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Tests.Services;

public class TopScorersServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;

    public TopScorersServiceTests()
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

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetTopScorersAsync_CalculatesAndSortsCorrectly()
    {
        // Arrange
        await using var context = GetDbContext();
        
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        var player1 = new Player { Id = 1, Name = "Player One", TeamId = 1 };
        var player2 = new Player { Id = 2, Name = "Player Two", TeamId = 1 };
        context.Teams.AddRange(team1, team2); // Add both teams
        context.Players.AddRange(player1, player2);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        var result1 = new Result { FixtureId = 1, HomeScore = 5, AwayScore = 0, Status = ResultStatus.Approved };
        context.Fixtures.Add(fixture1);
        context.Results.Add(result1);

        context.Goals.Add(new Goal { FixtureId = 1, PlayerId = 1 });
        context.Goals.Add(new Goal { FixtureId = 1, PlayerId = 1 });
        context.Goals.Add(new Goal { FixtureId = 1, PlayerId = 2 });
        context.Goals.Add(new Goal { FixtureId = 1, PlayerId = 2 });
        context.Goals.Add(new Goal { FixtureId = 1, PlayerId = 2 });

        await context.SaveChangesAsync();
        var service = new TopScorersService(context);

        // Act
        var topScorers = (await service.GetTopScorersAsync()).ToList();

        // Assert
        Assert.Equal(2, topScorers.Count);
        Assert.Equal("Player Two", topScorers[0].PlayerName);
        Assert.Equal(3, topScorers[0].Goals);
        Assert.Equal("Player One", topScorers[1].PlayerName);
        Assert.Equal(2, topScorers[1].Goals);
    }
    
    [Fact]
    public async Task GetTopScorersAsync_IgnoresGoalsFromNonApprovedResults()
    {
        // Arrange
        await using var context = GetDbContext();
        
        var team1 = new Team { Id = 1, Name = "Team A" };
        // *** FIX: Added Team B ***
        var team2 = new Team { Id = 2, Name = "Team B" };
        var player1 = new Player { Id = 1, Name = "Player One", TeamId = 1 };
        context.Teams.AddRange(team1, team2); // Add both teams
        context.Players.Add(player1);

        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        var result1 = new Result { FixtureId = 1, HomeScore = 1, AwayScore = 0, Status = ResultStatus.PendingApproval };
        context.Fixtures.Add(fixture1);
        context.Results.Add(result1);
        context.Goals.Add(new Goal { FixtureId = 1, PlayerId = 1 });

        await context.SaveChangesAsync();
        var service = new TopScorersService(context);

        // Act
        var topScorers = (await service.GetTopScorersAsync()).ToList();

        // Assert
        Assert.Empty(topScorers);
    }
}