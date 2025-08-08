using AutoMapper;
using LeagueManager.Infrastructure.Data;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.MappingProfiles;

namespace LeagueManager.Tests.Services;

public class LeagueConfigurationServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly IMapper _mapper;
    private bool _disposed;

    public LeagueConfigurationServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<LeagueDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Set up a real AutoMapper instance for the tests
        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile());
        });
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
    public async Task GetConfigurationAsync_WhenNoConfigExists_CreatesAndReturnsDefault()
    {
        // Arrange
        await using var context = GetDbContext();
        var service = new LeagueConfigurationService(context, _mapper);

        // Act
        var result = await service.GetConfigurationAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.MinPlayersPerTeam); // Check default value
        Assert.Equal(1, await context.LeagueConfigurations.CountAsync()); // Verify it was saved
    }

    [Fact]
    public async Task GetConfigurationAsync_WhenConfigExists_ReturnsExisting()
    {
        // Arrange
        await using var context = GetDbContext();
        context.LeagueConfigurations.Add(new LeagueConfiguration { MinPlayersPerTeam = 10 });
        await context.SaveChangesAsync();
        
        var service = new LeagueConfigurationService(context, _mapper);

        // Act
        var result = await service.GetConfigurationAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.MinPlayersPerTeam);
    }

    [Fact]
    public async Task UpdateConfigurationAsync_WhenConfigExists_UpdatesAndReturnsDto()
    {
        // Arrange
        await using var context = GetDbContext();
        context.LeagueConfigurations.Add(new LeagueConfiguration { MinPlayersPerTeam = 5 });
        await context.SaveChangesAsync();

        var service = new LeagueConfigurationService(context, _mapper);
        var dto = new LeagueConfigurationDto { MinPlayersPerTeam = 7 };

        // Act
        var result = await service.UpdateConfigurationAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.MinPlayersPerTeam);
        var dbConfig = await context.LeagueConfigurations.FirstAsync();
        Assert.Equal(7, dbConfig.MinPlayersPerTeam);
    }
}