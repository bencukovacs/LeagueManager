using LeagueManager.API.Controllers;
using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Tests.Controllers;

public class ResultsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;

    public ResultsControllerTests()
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
    public async Task UpdateResultStatus_WithValidData_ReturnsNoContent()
    {
        // Arrange
        await using var context = GetDbContext();
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        context.Teams.AddRange(team1, team2);
        var fixture = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        context.Fixtures.Add(fixture);
        var dbResult = new Result { Id = 1, FixtureId = 1, Status = ResultStatus.PendingApproval, HomeScore = 1, AwayScore = 0 };
        context.Results.Add(dbResult);
        await context.SaveChangesAsync();

        var controller = new ResultsController(context);
        var statusDto = new UpdateResultStatusDto { Status = ResultStatus.Approved };

        // Act
        var result = await controller.UpdateResultStatus(1, statusDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedResult = await context.Results.FindAsync(1);
        Assert.Equal(ResultStatus.Approved, updatedResult?.Status);
    }

    [Fact]
    public async Task UpdateResultStatus_WithInvalidResultId_ReturnsNotFound()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new ResultsController(context);
        var statusDto = new UpdateResultStatusDto { Status = ResultStatus.Approved };

        // Act
        var result = await controller.UpdateResultStatus(99, statusDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}