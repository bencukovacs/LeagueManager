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

public class RosterRequestServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly IMapper _mapper;
  private bool _disposed;

  public RosterRequestServiceTests()
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

    private Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string userId)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });
        return mockHttpContextAccessor;
    }

    [Fact]
    public async Task GetMyPendingRequestsAsync_WhenUserHasPendingRequests_ReturnsOnlyTheirPendingRequests()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { FullName = "Test User", Id = "user-123", UserName = "testuser" };
        // We must create the second user in the database to satisfy the foreign key.
        var anotherUser = new User { FullName = "Test User 2", Id = "another-user", UserName = "anotheruser" };
        var team = new Team { Id = 1, Name = "Test Team" };
        context.Users.AddRange(user, anotherUser); // Add both users
        context.Teams.Add(team);

        context.RosterRequests.AddRange(
            new RosterRequest { UserId = "user-123", TeamId = 1, Status = RosterRequestStatus.PendingLeaderApproval },
            new RosterRequest { UserId = "user-123", TeamId = 1, Status = RosterRequestStatus.PendingPlayerAcceptance },
            new RosterRequest { UserId = "user-123", TeamId = 1, Status = RosterRequestStatus.Approved }, // Should be ignored
            new RosterRequest { UserId = "another-user", TeamId = 1, Status = RosterRequestStatus.PendingLeaderApproval } // Should be ignored
        );
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new RosterRequestService(context, _mapper, mockHttpContextAccessor.Object);

        // Act
        var result = (await service.GetMyPendingRequestsAsync()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public async Task CreateJoinRequestAsync_WhenUserHasAnotherPendingRequest_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", FullName = "Test User" };
        var team1 = new Team { Id = 1, Name = "Team A" };
        var team2 = new Team { Id = 2, Name = "Team B" };
        context.Users.Add(user);
        context.Teams.AddRange(team1, team2);
        
        // Create an existing pending request to Team A
        context.RosterRequests.Add(new RosterRequest 
        { 
            UserId = "user-123", 
            TeamId = 1, 
            Status = RosterRequestStatus.PendingLeaderApproval 
        });
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new RosterRequestService(context, _mapper, mockHttpContextAccessor.Object);

        // Act & Assert
        // Try to create a new request to Team B
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateJoinRequestAsync(2));
        Assert.Equal("You already have a pending request to join a team and cannot send another.", exception.Message);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenUserManagesPendingTeam_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", FullName = "Test User" };
        var pendingTeam = new Team { Id = 1, Name = "My Pending Team", Status = TeamStatus.PendingApproval };
        var approvedTeam = new Team { Id = 2, Name = "Approved Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };
        context.Users.Add(user);
        context.Teams.AddRange(pendingTeam, approvedTeam);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new RosterRequestService(context, _mapper, mockHttpContextAccessor.Object);

        // Act & Assert
        // Try to join the approved team
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateJoinRequestAsync(2));
        Assert.Equal("You cannot join a team while your own team application is pending approval.", exception.Message);
    }
}