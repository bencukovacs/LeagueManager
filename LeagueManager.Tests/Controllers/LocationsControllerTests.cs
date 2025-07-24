using LeagueManager.API.Controllers;
using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Tests.Controllers;

public class LocationsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;

    public LocationsControllerTests()
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
    public async Task GetLocation_WithExistingId_ReturnsOkResult()
    {
        // Arrange
        await using var context = GetDbContext();
        context.Locations.Add(new Location { Id = 1, Name = "Main Pitch" });
        await context.SaveChangesAsync();
        var controller = new LocationsController(context);

        // Act
        var result = await controller.GetLocation(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var location = Assert.IsType<Location>(okResult.Value);
        Assert.Equal("Main Pitch", location.Name);
    }

    [Fact]
    public async Task CreateLocation_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new LocationsController(context);
        var dto = new LocationDto { Name = "Training Ground" };

        // Act
        var result = await controller.CreateLocation(dto);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(1, await context.Locations.CountAsync());
    }

    [Fact]
    public async Task DeleteLocation_WhenLocationIsUnused_ReturnsNoContent()
    {
        // Arrange
        await using var context = GetDbContext();
        context.Locations.Add(new Location { Id = 1, Name = "Unused Pitch" });
        await context.SaveChangesAsync();
        var controller = new LocationsController(context);

        // Act
        var result = await controller.DeleteLocation(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await context.Locations.CountAsync());
    }

    [Fact]
    public async Task DeleteLocation_WhenLocationIsInUse_ReturnsBadRequest()
    {
        // Arrange
        await using var context = GetDbContext();
        context.Teams.AddRange(
            new Team { Id = 1, Name = "Team A" },
            new Team { Id = 2, Name = "Team B" }
        );
        var location = new Location { Id = 1, Name = "Main Pitch" };
        context.Locations.Add(location);
        context.Fixtures.Add(new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2, LocationId = 1 });
        await context.SaveChangesAsync();
        
        var controller = new LocationsController(context);

        // Act
        var result = await controller.DeleteLocation(1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Cannot delete location as it is currently assigned to one or more fixtures.", badRequestResult.Value);
        Assert.Equal(1, await context.Locations.CountAsync()); // Ensure it was not deleted
    }
}