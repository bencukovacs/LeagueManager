using Moq;
using AutoMapper;
using LeagueManager.Infrastructure.Data;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using LeagueManager.Application.MappingProfiles;

namespace LeagueManager.Tests.Services;

public class PlayerServiceTests : IDisposable
{
  private readonly SqliteConnection _connection;
  private readonly DbContextOptions<LeagueDbContext> _options;
  private readonly IMapper _mapper;
  private bool _disposed;

  public PlayerServiceTests()
  {
    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();
    _options = new DbContextOptionsBuilder<LeagueDbContext>()
        .UseSqlite(_connection)
        .Options;

    var mappingConfig = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
    _mapper = mappingConfig.CreateMapper();

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

  private Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string? userId, string? role = null)
  {
    var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    if (userId != null)
    {
      var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
      if (role != null)
      {
        claims.Add(new Claim(ClaimTypes.Role, role));
      }
      var identity = new ClaimsIdentity(claims);
      var claimsPrincipal = new ClaimsPrincipal(identity);
      mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });
    }
    else
    {
      mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());
    }
    return mockHttpContextAccessor;
  }

  [Fact]
  public async Task GetUnassignedPlayersAsync_ReturnsOnlyPlayersWithNullTeamId()
  {
    // Arrange
    await using var context = GetDbContext();
    var team1 = new Team { Id = 1, Name = "Team A" };
    context.Teams.Add(team1);

    context.Players.AddRange(
        new Player { Name = "Assigned Player", TeamId = 1 },
        new Player { Name = "Unassigned Player 1", TeamId = null },
        new Player { Name = "Unassigned Player 2", TeamId = null }
    );
    await context.SaveChangesAsync();

    var service = new PlayerService(context, _mapper, new Mock<IHttpContextAccessor>().Object);

    // Act
    var result = (await service.GetUnassignedPlayersAsync()).ToList();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.Count);
  }

  [Fact]
  public async Task RemovePlayerAsync_AsAdmin_PermanentlyDeletesPlayer()
  {
    // Arrange
    await using var context = GetDbContext();
    // We must create the Team first.
    var team = new Team { Id = 1, Name = "Test Team" };
    context.Teams.Add(team);
    context.Players.Add(new Player { Id = 1, Name = "Test Player", TeamId = 1 });
    await context.SaveChangesAsync();

    var mockHttpContextAccessor = CreateMockHttpContextAccessor("admin-user", "Admin");
    var service = new PlayerService(context, _mapper, mockHttpContextAccessor.Object);

    // Act
    await service.DeletePlayerPermanentlyAsync(1);

    // Assert
    var playerCount = await context.Players.CountAsync();
    Assert.Equal(0, playerCount);
  }

  [Fact]
  public async Task RemovePlayerAsync_AsTeamLeader_SetsTeamIdToNull()
  {
    // Arrange
    await using var context = GetDbContext();
    var user = new User {FullName = "Test User", Id = "leader-user" };
    var team = new Team { Id = 1, Name = "My Team" };
    var player = new Player { Id = 1, Name = "Test Player", TeamId = 1 };
    var membership = new TeamMembership { UserId = "leader-user", TeamId = 1, Role = TeamRole.Leader };
    context.Users.Add(user);
    context.Teams.Add(team);
    context.Players.Add(player);
    context.TeamMemberships.Add(membership);
    await context.SaveChangesAsync();

    var mockHttpContextAccessor = CreateMockHttpContextAccessor("leader-user");
    var service = new PlayerService(context, _mapper, mockHttpContextAccessor.Object);

    // Act
    await service.RemovePlayerFromRosterAsync(1);

    // Assert
    var playerInDb = await context.Players.FindAsync(1);
    Assert.NotNull(playerInDb);
    Assert.Null(playerInDb.TeamId);
  }

  [Fact]
  public async Task AssignPlayerToTeamAsync_WhenSuccessful_UpdatesPlayerTeamId()
  {
    // Arrange
    await using var context = GetDbContext();
    var team = new Team { Id = 1, Name = "Team A", Status = TeamStatus.PendingApproval };
    var player = new Player { Id = 1, Name = "Free Agent", TeamId = null };
    context.Teams.Add(team);
    context.Players.Add(player);
    await context.SaveChangesAsync();

    var service = new PlayerService(context, _mapper, new Mock<IHttpContextAccessor>().Object);

    // Act
    var result = await service.AssignPlayerToTeamAsync(1, 1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.TeamId); // Check the returned DTO
    var playerInDb = await context.Players.FindAsync(1);
    Assert.NotNull(playerInDb);
    Assert.Equal(1, playerInDb.TeamId); // Check the database
  }
  
  [Fact]
    public async Task AssignPlayerToTeamAsync_WhenTeamNotFound_ReturnsNull()
    {
        // Arrange
        await using var context = GetDbContext();
        var player = new Player { Id = 1, Name = "Free Agent", TeamId = null };
        context.Players.Add(player);
        await context.SaveChangesAsync();
        var service = new PlayerService(context, _mapper, new Mock<IHttpContextAccessor>().Object);

        // Act
        var result = await service.AssignPlayerToTeamAsync(1, 99); // Non-existent team

        // Assert
        Assert.Null(result);
    }
    
  [Fact]
  public async Task DeletePlayerPermanentlyAsync_WhenPlayerIsLinkedToUser_ThrowsException()
  {
    // --- ARRANGE ---
    await using var context = GetDbContext();
    var user = new User { FullName = "User 123", Id = "user-123" };
    var team = new Team { Id = 1, Name = "Test Team" };
    // Create a player that IS linked to a user
    var player = new Player { Id = 1, Name = "Linked Player", TeamId = 1, UserId = "user-123" };
    context.Users.Add(user);
    context.Teams.Add(team);
    context.Players.Add(player);
    await context.SaveChangesAsync();

    var service = new PlayerService(context, _mapper, new Mock<IHttpContextAccessor>().Object);

    // --- ACT & ASSERT ---
    // Verify that the service call throws the correct exception
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeletePlayerPermanentlyAsync(1));
    Assert.Equal("Cannot permanently delete a player who is linked to a registered user account. Please remove them from the roster instead.", exception.Message);
  }
}