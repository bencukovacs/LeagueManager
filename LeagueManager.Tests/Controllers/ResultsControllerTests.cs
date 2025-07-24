using LeagueManager.API.Controllers;
using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

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

    private async Task SeedDataForResults(LeagueDbContext context)
    {
        var team1 = new Team { Id = 1, Name = "Team One" };
        var team2 = new Team { Id = 2, Name = "Team Two" };
        context.Teams.AddRange(team1, team2);

        var player1 = new Player { Id = 1, Name = "Player T1", TeamId = 1 };
        var player2 = new Player { Id = 2, Name = "Player T2", TeamId = 2 };
        context.Players.AddRange(player1, player2);

        var fixture = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2, KickOffDateTime = DateTime.UtcNow };
        context.Fixtures.Add(fixture);

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task SubmitResult_WithValidData_ReturnsOkResult()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedDataForResults(context);
        var controller = new ResultsController(context);
        var submitDto = new SubmitResultDto
        {
            HomeScore = 1,
            AwayScore = 0,
            Goalscorers = new List<GoalscorerDto> { new GoalscorerDto { PlayerId = 1 } }
        };

        // Act
        var result = await controller.SubmitResult(1, submitDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var createdResult = Assert.IsType<Result>(okResult.Value);
        Assert.Equal(ResultStatus.PendingApproval, createdResult.Status);

        var fixture = await context.Fixtures.FindAsync(1);
        Assert.Equal(FixtureStatus.Completed, fixture?.Status);
        Assert.Equal(1, await context.Results.CountAsync());
        Assert.Equal(1, await context.Goals.CountAsync());
    }
    
    [Fact]
    public async Task SubmitResult_ForNonExistentFixture_ReturnsNotFound()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedDataForResults(context);
        var controller = new ResultsController(context);
        var submitDto = new SubmitResultDto();

        // Act
        var result = await controller.SubmitResult(99, submitDto); // Invalid Fixture ID

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Fixture not found.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task SubmitResult_WhenResultAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedDataForResults(context);
        context.Results.Add(new Result { FixtureId = 1, HomeScore = 1, AwayScore = 0 });
        await context.SaveChangesAsync();

        var controller = new ResultsController(context);
        var submitDto = new SubmitResultDto();

        // Act
        var result = await controller.SubmitResult(1, submitDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("A result for this fixture has already been submitted.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task SubmitResult_WhenScoreMismatchesGoalscorers_ReturnsBadRequest()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedDataForResults(context);
        var controller = new ResultsController(context);
        var submitDto = new SubmitResultDto
        {
            HomeScore = 2, // Score is 2
            AwayScore = 0,
            Goalscorers = new List<GoalscorerDto> { new() { PlayerId = 1 } } // But only 1 goalscorer
        };
        
        // Act
        var result = await controller.SubmitResult(1, submitDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The number of goalscorers does not match the total score.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateResultStatus_WithValidData_ReturnsNoContent()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedDataForResults(context);
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
        await SeedDataForResults(context);
        var controller = new ResultsController(context);
        var statusDto = new UpdateResultStatusDto { Status = ResultStatus.Approved };

        // Act
        var result = await controller.UpdateResultStatus(99, statusDto); // Invalid Result ID

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
