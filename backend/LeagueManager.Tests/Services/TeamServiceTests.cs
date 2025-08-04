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

    // ... (Your other TeamService tests like CreateTeamAsync and ApproveTeamAsync remain here) ...

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
}