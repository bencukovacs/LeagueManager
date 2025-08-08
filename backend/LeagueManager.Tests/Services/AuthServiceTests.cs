using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Services;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Settings;
using LeagueManager.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using Moq;

namespace LeagueManager.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LeagueDbContext> _options;
    private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;

  public AuthServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<LeagueDbContext>().UseSqlite(_connection).Options;
        using var context = new LeagueDbContext(_options);
        context.Database.EnsureCreated();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<LeagueDbContext>(o => o.UseSqlite(_connection));
        services.AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<LeagueDbContext>();
        _serviceProvider = services.BuildServiceProvider();
        
        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
        var jwtSettings = new JwtSettings { Key = "this_is_a_super_secret_key_for_testing_1234567890", Issuer = "test", Audience = "test" };
        _mockJwtSettings.Setup(s => s.Value).Returns(jwtSettings);
    }

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
    public async Task RegisterUserAsync_WhenSuccessful_CreatesUserAndPlayerInDatabase()
    {
        // Arrange
        await using var context = new LeagueDbContext(_options);
        var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var service = new AuthService(userManager, _mockJwtSettings.Object, context);
        var registerDto = new RegisterDto { FirstName = "Test", LastName = "User", Email = "test@example.com", Password = "Password123!" };
        
        await roleManager.CreateAsync(new IdentityRole("RegisteredUser"));
        
        // Act
        var result = await service.RegisterUserAsync(registerDto);

        // Assert
        Assert.True(result.Succeeded);
        var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(userInDb);
        var playerInDb = await context.Players.FirstOrDefaultAsync(p => p.UserId == userInDb.Id);
        Assert.NotNull(playerInDb);
    }
    
    [Fact]
    public async Task LoginUserAsync_WithValidCredentials_ReturnsJwtTokenWithRoles()
    {
        // Arrange
        await using var context = new LeagueDbContext(_options);
        var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var service = new AuthService(userManager, _mockJwtSettings.Object, context);
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
        
        await roleManager.CreateAsync(new IdentityRole("Admin"));
        var user = new User { UserName = "test@example.com", Email = "test@example.com" };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        // Act
        var tokenString = await service.LoginUserAsync(loginDto);

        // Assert
        Assert.NotNull(tokenString);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == "role");
        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);
    }

    [Fact]
    public async Task LoginUserAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        await using var context = new LeagueDbContext(_options);
        var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        var service = new AuthService(userManager, _mockJwtSettings.Object, context);
        var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword!" };
        
        var user = new User { UserName = "test@example.com", Email = "test@example.com" };
        await userManager.CreateAsync(user, "CorrectPassword123!");

        // Act
        var token = await service.LoginUserAsync(loginDto);

        // Assert
        Assert.Null(token);
    }
    
    [Fact]
    public async Task LoginUserAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        await using var context = new LeagueDbContext(_options);
        var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        var service = new AuthService(userManager, _mockJwtSettings.Object, context);
        var loginDto = new LoginDto { Email = "nosuchuser@example.com", Password = "Password123!" };

        // Act
        var token = await service.LoginUserAsync(loginDto);

        // Assert
        Assert.Null(token);
    }
}