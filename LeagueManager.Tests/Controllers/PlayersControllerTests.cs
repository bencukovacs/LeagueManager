using LeagueManager.API.Controllers;
using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace LeagueManager.Tests.Controllers;

public class PlayersControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;

    public PlayersControllerTests()
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
    public async Task GetPlayers_ReturnsOkResult_WithListOfPlayers()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        context.Teams.Add(team);
        context.Players.Add(new Player { Name = "Player One", TeamId = 1 });
        context.Players.Add(new Player { Name = "Player Two", TeamId = 1 });
        await context.SaveChangesAsync();
        
        var controller = new PlayersController(context);

        // Act
        var result = await controller.GetPlayers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var players = Assert.IsAssignableFrom<IEnumerable<Player>>(okResult.Value);
        Assert.Equal(2, players.Count());
    }
    
    [Fact]
    public async Task GetPlayer_WithExistingId_ReturnsOkResult_WithPlayer()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        context.Teams.Add(team);
        var player = new Player { Id = 1, Name = "Test Player", TeamId = 1 };
        context.Players.Add(player);
        await context.SaveChangesAsync();

        var controller = new PlayersController(context);

        // Act
        var result = await controller.GetPlayer(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPlayer = Assert.IsType<Player>(okResult.Value);
        Assert.Equal(1, returnedPlayer.Id);
    }
    
    [Fact]
    public async Task GetPlayer_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new PlayersController(context);

        // Act
        var result = await controller.GetPlayer(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task CreatePlayer_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        
        var controller = new PlayersController(context);
        var createDto = new PlayerDto { Name = "New Player", TeamId = 1 };

        // Act
        var result = await controller.CreatePlayer(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdPlayer = Assert.IsType<Player>(createdAtActionResult.Value);
        Assert.Equal("New Player", createdPlayer.Name);
        Assert.Equal(1, await context.Players.CountAsync());
    }
    
    [Fact]
    public async Task CreatePlayer_WithInvalidTeamId_ReturnsBadRequest()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new PlayersController(context);
        var createDto = new PlayerDto { Name = "New Player", TeamId = 99 }; // Invalid Team ID

        // Act
        var result = await controller.CreatePlayer(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Team ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeletePlayer_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        context.Teams.Add(team);
        var player = new Player { Id = 1, Name = "Player to Delete", TeamId = 1 };
        context.Players.Add(player);
        await context.SaveChangesAsync();
        
        var controller = new PlayersController(context);

        // Act
        var result = await controller.DeletePlayer(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await context.Players.CountAsync());
    }
}