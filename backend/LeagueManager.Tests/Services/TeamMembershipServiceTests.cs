using AutoMapper;
using LeagueManager.Infrastructure.Data;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.MappingProfiles;

namespace LeagueManager.Tests.Services;

public class TeamMembershipServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly IMapper _mapper;
    private bool _disposed;

    public TeamMembershipServiceTests()
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

    [Fact]
    public async Task GetMembersForTeamAsync_ReturnsCorrectMembers()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        var user1 = new User { FullName = "User 1", Id = "user-1", UserName = "User One" };
        var user2 = new User { FullName = "User 2", Id = "user-2", UserName = "User Two" };
        context.Teams.Add(team);
        context.Users.AddRange(user1, user2);
        context.TeamMemberships.AddRange(
            new TeamMembership { TeamId = 1, UserId = "user-1", Role = TeamRole.Leader },
            new TeamMembership { TeamId = 1, UserId = "user-2", Role = TeamRole.Member }
        );
        await context.SaveChangesAsync();
        var service = new TeamMembershipService(context, _mapper);

        // Act
        var result = (await service.GetMembersForTeamAsync(1)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.UserName == "User One" && r.Role == "Leader");
        Assert.Contains(result, r => r.UserName == "User Two" && r.Role == "Member");
    }

    [Fact]
    public async Task UpdateMemberRoleAsync_WhenMembershipExists_UpdatesRole()
    {
        // Arrange
        await using var context = GetDbContext();
        var team = new Team { Id = 1, Name = "Test Team" };
        var user = new User { FullName = "User 1", Id = "user-1", UserName = "User One" };
        var membership = new TeamMembership { TeamId = 1, UserId = "user-1", Role = TeamRole.Member };
        context.Teams.Add(team);
        context.Users.Add(user);
        context.TeamMemberships.Add(membership);
        await context.SaveChangesAsync();
        
        var service = new TeamMembershipService(context, _mapper);
        var dto = new UpdateTeamMemberRoleDto { NewRole = TeamRole.AssistantLeader };

        // Act
        var result = await service.UpdateMemberRoleAsync(1, "user-1", dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AssistantLeader", result.Role);
        var membershipInDb = await context.TeamMemberships.FirstAsync();
        Assert.Equal(TeamRole.AssistantLeader, membershipInDb.Role);
    }
}