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
using LeagueManager.Application.Services;
using LeagueManager.Application.MappingProfiles;

namespace LeagueManager.Tests.Services;

public class TeamServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly IMapper _mapper;
    private readonly Mock<ILeagueConfigurationService> _mockConfigService;
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
        
        _mockConfigService = new Mock<ILeagueConfigurationService>();

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
    public async Task GetMyTeamAndConfigAsync_WhenUserIsTeamLeader_ReturnsCorrectData()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { FullName = "Test User", Id = "user-123", UserName = "testuser" };
        var team1 = new Team { Id = 1, Name = "My Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };
        context.Users.Add(user);
        context.Teams.Add(team1);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 5 };
        _mockConfigService.Setup(s => s.GetConfigurationAsync()).ReturnsAsync(configDto);

        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act
        var result = await service.GetMyTeamAndConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MyTeam);
        Assert.Equal(1, result.MyTeam.Team.Id);
        Assert.Equal("Leader", result.MyTeam.UserRole);
        Assert.Equal(5, result.Config.MinPlayersPerTeam);
    }

    [Fact]
    public async Task GetMyTeamAndConfigAsync_WhenUserIsRegularMember_ReturnsCorrectData()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { FullName = "User 456", Id = "user-456", UserName = "testmember" };
        var team1 = new Team { Id = 1, Name = "My Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-456", TeamId = 1, Role = TeamRole.Member };
        context.Users.Add(user);
        context.Teams.Add(team1);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-456");
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 5 };
        _mockConfigService.Setup(s => s.GetConfigurationAsync()).ReturnsAsync(configDto);

        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act
        var result = await service.GetMyTeamAndConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MyTeam);
        Assert.Equal(1, result.MyTeam.Team.Id);
        Assert.Equal("Member", result.MyTeam.UserRole);
    }

    [Fact]
    public async Task GetMyTeamAndConfigAsync_WhenUserIsNotOnATeam_ReturnsConfigOnly()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { FullName = "User 789", Id = "user-789", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-789");
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 5 };
        _mockConfigService.Setup(s => s.GetConfigurationAsync()).ReturnsAsync(configDto);
        
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act
        var result = await service.GetMyTeamAndConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.MyTeam);
        Assert.NotNull(result.Config);
        Assert.Equal(5, result.Config.MinPlayersPerTeam);
    }
    
    [Fact]
    public async Task ApproveTeamAsync_WhenTeamDoesNotMeetPlayerRequirement_ThrowsException()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Pending Team", Status = TeamStatus.PendingApproval, PrimaryColor = "Blue" };
        context.Teams.Add(team);
        // Add only 4 players
        for (int i = 0; i < 4; i++)
        {
            context.Players.Add(new Player { Name = $"Player {i}", TeamId = 1 });
        }
        await context.SaveChangesAsync();
        
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 5 };
        _mockConfigService.Setup(s => s.GetConfigurationAsync()).ReturnsAsync(configDto);

        var service = new TeamService(context, _mapper, new Mock<IHttpContextAccessor>().Object, _mockConfigService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveTeamAsync(1));
        Assert.Contains("requires at least 5", exception.Message);
    }
    
    [Fact]
    public async Task CreateTeamAsAdminAsync_CreatesTeamWithApprovedStatus()
    {
        // Arrange
        await using var context = GetDbContext();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);
        var dto = new CreateTeamDto { Name = "Admin Created Team", PrimaryColor = "Black" };

        // Act
        var result = await service.CreateTeamAsAdminAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
        var teamInDb = await context.Teams.FirstOrDefaultAsync(t => t.Name == "Admin Created Team");
        Assert.NotNull(teamInDb);
        Assert.Equal(TeamStatus.Approved, teamInDb.Status);
    }
    
    [Fact]
    public async Task GetFixturesForMyTeamAsync_WhenUserIsTeamLeader_ReturnsOnlyTheirFixtures()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { FullName = "User 123", Id = "user-123", UserName = "testuser" };
        var team1 = new Team { Id = 1, Name = "My Team" };
        var team2 = new Team { Id = 2, Name = "Opponent Team" };
        var team3 = new Team { Id = 3, Name = "Other Team" };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };
        var fixture1 = new Fixture { Id = 1, HomeTeamId = 1, AwayTeamId = 2 };
        var fixture2 = new Fixture { Id = 2, HomeTeamId = 2, AwayTeamId = 1 };
        var fixture3 = new Fixture { Id = 3, HomeTeamId = 2, AwayTeamId = 3 };
        context.Users.Add(user);
        context.Teams.AddRange(team1, team2, team3);
        context.TeamMemberships.Add(membership);
        context.Fixtures.AddRange(fixture1, fixture2, fixture3);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act
        var result = (await service.GetFixturesForMyTeamAsync()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public async Task CreateTeamAsync_WhenTeamNameExists_ThrowsInvalidOperationException()
    {
        // --- ARRANGE ---
        // 1. Set up the database with an existing team
        await using var context = GetDbContext();
        context.Teams.Add(new Team { Id = 1, Name = "Existing Team" });
        await context.SaveChangesAsync();

        // 2. Set up a mock user and the service
        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);
    
        // 3. Create a DTO with a duplicate name (case-insensitive)
        var dto = new CreateTeamDto { Name = "existing team" };

        // --- ACT & ASSERT ---
        // 4. Verify that the service call throws the correct exception
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTeamAsync(dto));
        Assert.Equal("A team with this name already exists.", exception.Message);
    }
    
    [Fact]
    public async Task LeaveMyTeamAsync_WhenLeaderCancelsPendingTeam_DeletesTeamAndMembership()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", FullName = "Test User" };
        var team = new Team { Id = 1, Name = "Pending Team", Status = TeamStatus.PendingApproval };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };
        context.Users.Add(user);
        context.Teams.Add(team);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act
        await service.LeaveMyTeamAsync();

        // Assert
        Assert.Equal(0, await context.Teams.CountAsync());
        Assert.Equal(0, await context.TeamMemberships.CountAsync());
    }

    [Fact]
    public async Task LeaveMyTeamAsync_WhenMemberLeavesApprovedTeam_DeletesMembershipOnly()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", FullName = "Test User" };
        var team = new Team { Id = 1, Name = "Approved Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Member };
        context.Users.Add(user);
        context.Teams.Add(team);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act
        await service.LeaveMyTeamAsync();

        // Assert
        Assert.Equal(1, await context.Teams.CountAsync()); // Team should still exist
        Assert.Equal(0, await context.TeamMemberships.CountAsync()); // Membership should be gone
    }

    [Fact]
    public async Task LeaveMyTeamAsync_WhenLastLeaderTriesToLeaveApprovedTeam_ThrowsException()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "user-123", FullName = "Test User" };
        var team = new Team { Id = 1, Name = "Approved Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "user-123", TeamId = 1, Role = TeamRole.Leader };
        context.Users.Add(user);
        context.Teams.Add(team);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("user-123");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.LeaveMyTeamAsync());
        Assert.Equal("You are the last manager of this team. You must transfer leadership before leaving.", exception.Message);
    }

    [Fact]
    public async Task DisbandMyTeamAsync_WhenLeaderDisbandsApprovedTeam_DeletesTeamAndMemberships()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "leader-user", FullName = "Test Leader" };
        var team = new Team { Id = 1, Name = "Approved Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "leader-user", TeamId = 1, Role = TeamRole.Leader };
        var player = new Player { Name = "Test Player", TeamId = 1 };
        context.Users.Add(user);
        context.Teams.Add(team);
        context.TeamMemberships.Add(membership);
        context.Players.Add(player);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("leader-user");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act
        await service.DisbandMyTeamAsync();

        // Assert
        Assert.Equal(0, await context.Teams.CountAsync());
        Assert.Equal(0, await context.TeamMemberships.CountAsync());
        var playerInDb = await context.Players.FirstAsync();
        Assert.Null(playerInDb.TeamId); // Verify player was soft-deleted
    }

    [Fact]
    public async Task DisbandMyTeamAsync_WhenNotLeader_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        await using var context = GetDbContext();
        var user = new User { Id = "member-user", FullName = "Test Member" };
        var team = new Team { Id = 1, Name = "Approved Team", Status = TeamStatus.Approved };
        var membership = new TeamMembership { UserId = "member-user", TeamId = 1, Role = TeamRole.Member };
        context.Users.Add(user);
        context.Teams.Add(team);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();

        var mockHttpContextAccessor = CreateMockHttpContextAccessor("member-user");
        var service = new TeamService(context, _mapper, mockHttpContextAccessor.Object, _mockConfigService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DisbandMyTeamAsync());
    }
}