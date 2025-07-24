using LeagueManager.API.Controllers;
using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace LeagueManager.Tests.Controllers;

public class FixturesControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;

    public FixturesControllerTests()
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

    private async Task SeedTeamsAndLocation(LeagueDbContext context)
    {
        context.Teams.AddRange(
            new Team { Id = 1, Name = "Team One" },
            new Team { Id = 2, Name = "Team Two" }
        );
        context.Locations.Add(new Location { Id = 1, Name = "Main Pitch" });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetFixtures_ReturnsOkResult_WithListOfFixtures()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedTeamsAndLocation(context);
        context.Fixtures.Add(new Fixture { HomeTeamId = 1, AwayTeamId = 2, KickOffDateTime = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var controller = new FixturesController(context);

        // Act
        var result = await controller.GetFixtures();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var fixtures = Assert.IsAssignableFrom<IEnumerable<Fixture>>(okResult.Value);
        Assert.Single(fixtures);
    }

    [Fact]
    public async Task CreateFixture_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedTeamsAndLocation(context);
        var controller = new FixturesController(context);
        var createDto = new CreateFixtureDto
        {
            HomeTeamId = 1,
            AwayTeamId = 2,
            LocationId = 1,
            KickOffDateTime = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await controller.CreateFixture(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdFixture = Assert.IsType<Fixture>(createdAtActionResult.Value);
        Assert.Equal(1, createdFixture.HomeTeamId);
        Assert.Equal(1, await context.Fixtures.CountAsync());
    }

    [Fact]
    public async Task CreateFixture_WithSameHomeAndAwayTeam_ReturnsBadRequest()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedTeamsAndLocation(context);
        var controller = new FixturesController(context);
        var createDto = new CreateFixtureDto { HomeTeamId = 1, AwayTeamId = 1 }; // Same team

        // Act
        var result = await controller.CreateFixture(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Home team and away team cannot be the same.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task CreateFixture_WithInvalidHomeTeamId_ReturnsBadRequest()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedTeamsAndLocation(context);
        var controller = new FixturesController(context);
        var createDto = new CreateFixtureDto { HomeTeamId = 99, AwayTeamId = 1 }; // Invalid home team

        // Act
        var result = await controller.CreateFixture(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("One or both teams do not exist.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateFixture_WithValidData_ReturnsNoContent()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedTeamsAndLocation(context);
        var originalDate = DateTime.UtcNow;
        var fixture = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2, KickOffDateTime = originalDate, LocationId = 1 };
        context.Fixtures.Add(fixture);
        await context.SaveChangesAsync();

        var controller = new FixturesController(context);
        var newDate = DateTime.UtcNow.AddDays(1);
        var updateDto = new UpdateFixtureDto { KickOffDateTime = newDate, LocationId = 1 };

        // Act
        var result = await controller.UpdateFixture(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedFixture = await context.Fixtures.FindAsync(1);
        Assert.Equal(newDate, updatedFixture?.KickOffDateTime);
    }
    
    [Fact]
    public async Task UpdateFixture_WithInvalidFixtureId_ReturnsNotFound()
    {
        // Arrange
        await using var context = GetDbContext();
        await SeedTeamsAndLocation(context);
        var controller = new FixturesController(context);
        var updateDto = new UpdateFixtureDto { KickOffDateTime = DateTime.UtcNow };

        // Act
        var result = await controller.UpdateFixture(99, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}