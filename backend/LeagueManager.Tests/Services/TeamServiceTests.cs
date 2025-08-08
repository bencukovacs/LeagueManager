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
using LeagueManager.Application.Dtos;

namespace LeagueManager.Tests.Services;

public class TeamServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly IMapper _mapper;
    private bool _disposed;

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
    public async Task GetMyTeamAsync_WhenUserIsTeamLeader_ReturnsCorrectTeamAndRole()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", UserName = "testuser" };
        var team1 = new Team { Id = 1, Name = "My Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };
        context.Users.Add(user);
        context.Teams.Add(team1);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = await service.GetMyTeamAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Team.Id);
        Assert.Equal("My Team", result.Team.Name);
        Assert.Equal("Leader", result.UserRole);
    }

    [Fact]
    public async Task GetMyTeamAsync_WhenUserIsRegularMember_ReturnsCorrectTeamAndRole()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-456", UserName = "testmember" };
        var team1 = new Team { Id = 1, Name = "My Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-456", TeamId = 1, Role = TeamRole.Member };
        context.Users.Add(user);
        context.Teams.Add(team1);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-456");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = await service.GetMyTeamAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Team.Id);
        Assert.Equal("Member", result.UserRole);
    }

    [Fact]
    public async Task GetMyTeamAsync_WhenUserIsNotOnATeam_ReturnsNull()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-789", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-789");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = await service.GetMyTeamAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFixturesForMyTeamAsync_WhenUserIsTeamLeader_ReturnsOnlyTheirFixtures()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", UserName = "testuser" };
        var team1 = new Team { Id = 1, Name = "My Team" };
        var team2 = new Team { Id = 2, Name = "Opponent Team" };
        var team3 = new Team { Id = 3, Name = "Other Team" };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };

        // Fixture 1: User's team is Home
        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        // Fixture 2: User's team is Away
        var fixture2 = new Fixture { Id = 2, HomeTeamId = 2, AwayTeamId = 1 };
        // Fixture 3: Should be ignored
        var fixture3 = new Fixture { Id = 3, HomeTeamId = 2, AwayTeamId = 3 };

        context.Users.Add(user);
        context.Teams.AddRange(team1, team2, team3);
        context.TeamMemberships.Add(membership);
        context.Fixtures.AddRange(fixture1, fixture2, fixture3);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = (await service.GetFixturesForMyTeamAsync()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Should only return the two relevant fixtures
        Assert.Contains(result, f => f.Id == 1);
        Assert.Contains(result, f => f.Id == 2);
        Assert.DoesNotContain(result, f => f.Id == 3);
    }

    [Fact]
    public async Task GetFixturesForMyTeamAsync_WhenUserIsNotOnATeam_ReturnsEmptyList()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = await service.GetFixturesForMyTeamAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    [Fact]
    public async Task CreateTeamAsAdminAsync_CreatesTeamWithApprovedStatus()
    {
        // --- ARRANGE ---
        // We don't need a logged-in user for this test, so the HttpContextAccessor can be a simple mock.
        await using var context = GetDbContext();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object);
        var dto = new CreateTeamDto { Name = "Admin Created Team", PrimaryColor = "Black" };

        // --- ACT ---
        // Call the admin-specific creation method
        var result = await service.CreateTeamAsAdminAsync(dto);

        // --- ASSERT ---
        // 1. Verify the returned DTO is correct
        Assert.NotNull(result);
        Assert.Equal("Admin Created Team", result.Name);
        Assert.Equal("Approved", result.Status);

        // 2. Verify the entity in the database is correct
        var teamInDb = await context.Teams.FirstOrDefaultAsync(t => t.Name == "Admin Created Team");
        Assert.NotNull(teamInDb);
        Assert.Equal(TeamStatus.Approved, teamInDb.Status);

        // 3. Verify that NO TeamMembership record was created
        var membershipCount = await context.TeamMemberships.CountAsync();
        Assert.Equal(0, membershipCount);
    }
}