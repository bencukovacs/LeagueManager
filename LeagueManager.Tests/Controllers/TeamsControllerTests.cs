using LeagueManager.API.Controllers;
using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace LeagueManager.Tests.Controllers;

public class TeamsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;

    public TeamsControllerTests()
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
    public async Task GetTeams_ReturnsOkResult_WithListOfTeams()
    {
        // Arrange
        await using var context = GetDbContext();
        context.Teams.Add(new Team { Name = "Team A" });
        context.Teams.Add(new Team { Name = "Team B" });
        await context.SaveChangesAsync();
        
        var controller = new TeamsController(context);

        // Act
        var result = await controller.GetTeams();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var teams = Assert.IsAssignableFrom<IEnumerable<Team>>(okResult.Value);
        Assert.Equal(2, teams.Count());
    }
    
    [Fact]
    public async Task GetTeam_WithExistingId_ReturnsOkResult_WithTeam()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var controller = new TeamsController(context);

        // Act
        var result = await controller.GetTeam(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTeam = Assert.IsType<Team>(okResult.Value);
        Assert.Equal(1, returnedTeam.Id);
    }
    
    [Fact]
    public async Task GetTeam_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new TeamsController(context);

        // Act
        var result = await controller.GetTeam(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task CreateTeam_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new TeamsController(context);
        var createDto = new CreateTeamDto { Name = "New Team" };

        // Act
        var result = await controller.CreateTeam(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdTeam = Assert.IsType<Team>(createdAtActionResult.Value);
        Assert.Equal("New Team", createdTeam.Name);
        Assert.Equal(1, await context.Teams.CountAsync()); // Verify it was added to the db
    }

    [Fact]
    public async Task UpdateTeam_WithValidIdAndDto_ReturnsNoContent()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Old Name" };
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        
        var controller = new TeamsController(context);
        var updateDto = new CreateTeamDto { Name = "Updated Name" };

        // Act
        var result = await controller.UpdateTeam(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedTeam = await context.Teams.FindAsync(1);
        Assert.Equal("Updated Name", updatedTeam?.Name);
    }
    
    [Fact]
    public async Task UpdateTeam_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new TeamsController(context);
        var updateDto = new CreateTeamDto { Name = "Updated Name" };

        // Act
        var result = await controller.UpdateTeam(99, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteTeam_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Team to Delete" };
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        
        var controller = new TeamsController(context);

        // Act
        var result = await controller.DeleteTeam(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await context.Teams.CountAsync());
    }

    [Fact]
    public async Task DeleteTeam_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await using var context = GetDbContext();
        var controller = new TeamsController(context);

        // Act
        var result = await controller.DeleteTeam(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}