using Moq;
using AutoMapper;
using LeagueManager.Infrastructure.Data;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using LeagueManager.Application.MappingProfiles;

namespace LeagueManager.Tests.Services;

public class TeamServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly IMapper _mapper;

    public TeamServiceTests()
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

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string? userId)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        if (userId != null)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
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
    public async Task CreateTeamAsync_WhenUserIsAuthenticated_CreatesTeamAndMembership()
    {
        // Arrange
        await using var context = GetDbContext();
        
        // FIX #1: We must create the user in the database first to satisfy the foreign key.
        var user = new User { Id = "user-123", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);
        var dto = new CreateTeamDto { Name = "New Team" };

        // Act
        var result = await service.CreateTeamAsync(dto);

        // Assert
        Assert.NotNull(result);
        var membershipInDb = await context.TeamMemberships.FirstOrDefaultAsync();
        Assert.NotNull(membershipInDb);
        Assert.Equal("user-123", membershipInDb.UserId);
    }

    [Fact]
    public async Task ApproveTeamAsync_WhenTeamHasEnoughPlayersAndColor_ApprovesTeam()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Pending Team", Status = TeamStatus.PendingApproval, PrimaryColor = "Blue" };
        context.Teams.Add(team);

        // FIX #2: The service logic requires 6 players, so we must provide 6 in the test.
        for (int i = 0; i < 6; i++)
        {
            context.Players.Add(new Player { Name = $"Player {i}", TeamId = 1 });
        }
        await context.SaveChangesAsync();
        
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(null);
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = await service.ApproveTeamAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
    }

    // ... (the other tests in this file are unchanged) ...
    [Fact]
    public async Task GetMyTeamAsync_WhenUserIsTeamLeader_ReturnsCorrectTeam()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", UserName = "testuser" };
        var team1 = new Team { Id = 1, Name = "My Team", Status = TeamStatus.Approved };
        var team2 = new Team { Id = 2, Name = "Other Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };
        context.Users.Add(user);
        context.Teams.AddRange(team1, team2);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = await service.GetMyTeamAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetMyTeamAsync_WhenUserIsNotTeamLeader_ReturnsNull()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = await service.GetMyTeamAsync();

        // Assert
        Assert.Null(result);
    }
}