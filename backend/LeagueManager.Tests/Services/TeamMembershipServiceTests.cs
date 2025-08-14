using Moq;
using AutoMapper;
using LeagueManager.Infrastructure.Data;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.MappingProfiles;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LeagueManager.Tests.Services;

public class TeamMembershipServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly IMapper _mapper;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public TeamMembershipServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<LeagueDbContext>()
            .UseSqlite(_connection)
            .Options;

        var mappingConfig = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
        _mapper = mappingConfig.CreateMapper();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        using var context = new LeagueDbContext(_options);
        context.Database.EnsureCreated();
    }

    private LeagueDbContext GetDbContext() => new LeagueDbContext(_options);

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private void SetupMockUser(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });
    }

    [Fact]
    public async Task GetMembersForTeamAsync_ReturnsCorrectMembers()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        var user1 = new User { Id = "user-1", UserName = "user1@test.com", Email = "user1@test.com", FullName = "User One" };
        var user2 = new User { Id = "user-2", UserName = "user2@test.com", Email = "user2@test.com", FullName = "User Two" };
        
        // --- THIS IS THE FIX ---
        // We must explicitly add the parent entities to the context for the in-memory provider.
        context.Teams.Add(team);
        context.Users.AddRange(user1, user2);
        // --- END FIX ---

        context.TeamMemberships.AddRange(
            new TeamMembership { TeamId = 1, UserId = "user-1", Role = TeamRole.Leader },
            new TeamMembership { TeamId = 1, UserId = "user-2", Role = TeamRole.Member }
        );
        await context.SaveChangesAsync();
        
        var service = new TeamMembershipService(context, _mapper, _mockHttpContextAccessor.Object);

        // Act
        var result = (await service.GetMembersForTeamAsync(1)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Email == "user1@test.com" && r.Role == "Leader");
        Assert.Contains(result, r => r.FullName == "User Two" && r.Role == "Member");
    }

    [Fact]
    public async Task UpdateMemberRoleAsync_WhenLeaderUpdatesMember_Succeeds()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        var leader = new User { Id = "leader-1", UserName = "leader@test.com", Email = "leader@test.com", FullName = "Leader User" };
        var member = new User { Id = "member-1", UserName = "member@test.com", Email = "member@test.com", FullName = "Member User" };
        
        context.Teams.Add(team);
        context.Users.AddRange(leader, member);
        
        context.TeamMemberships.AddRange(
            new TeamMembership { TeamId = 1, UserId = "leader-1", Role = TeamRole.Leader },
            new TeamMembership { TeamId = 1, UserId = "member-1", Role = TeamRole.Member }
        );
        await context.SaveChangesAsync();
        
        SetupMockUser("leader-1");
        var service = new TeamMembershipService(context, _mapper, _mockHttpContextAccessor.Object);
        var dto = new UpdateTeamMemberRoleDto { NewRole = TeamRole.AssistantLeader };

        // Act
        var result = await service.UpdateMemberRoleAsync(1, "member-1", dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AssistantLeader", result.Role);
    }
}