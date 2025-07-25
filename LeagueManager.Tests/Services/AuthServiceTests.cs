using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using LeagueManager.Domain.Models;
using LeagueManager.Application.Services;
using LeagueManager.Infrastructure.Services;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Settings;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LeagueManager.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
{
    var store = new Mock<IUserStore<User>>();
    var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
    var passwordHasher = new Mock<IPasswordHasher<User>>();
    var userValidators = new List<IUserValidator<User>>();
    var passwordValidators = new List<IPasswordValidator<User>>();
    var keyNormalizer = new Mock<ILookupNormalizer>();
    var errors = new Mock<IdentityErrorDescriber>();
    var services = new Mock<IServiceProvider>();
    var logger = new Mock<ILogger<UserManager<User>>>();

    _mockUserManager = new Mock<UserManager<User>>(
        store.Object,
        optionsAccessor.Object,
        passwordHasher.Object,
        userValidators,
        passwordValidators,
        keyNormalizer.Object,
        errors.Object,
        services.Object,
        logger.Object);

    // Mock the JWT Settings
    _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
    var jwtSettings = new JwtSettings
    {
        Key = "a-very-long-and-secret-key-for-testing-purposes-only",
        Issuer = "test-issuer",
        Audience = "test-audience"
    };
    _mockJwtSettings.Setup(s => s.Value).Returns(jwtSettings);

    // Create the service instance with the mocked dependencies
    _authService = new AuthService(_mockUserManager.Object, _mockJwtSettings.Object);
}

    [Fact]
    public async Task RegisterUserAsync_WhenSuccessful_ReturnsSuccessResult()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Password = "Password123!" };
        
        // Setup the mock UserManager to return a success result when CreateAsync is called
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterUserAsync(registerDto);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task LoginUserAsync_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
        var user = new User { Id = "1", Email = "test@example.com", UserName = "test@example.com" };

        // Setup the mock UserManager to find a user and then successfully check the password
        _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);

        // Act
        var token = await _authService.LoginUserAsync(loginDto);

        // Assert
        Assert.NotNull(token);
        Assert.IsType<string>(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task LoginUserAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword!" };
        var user = new User { Id = "1", Email = "test@example.com", UserName = "test@example.com" };

        // Setup the mock UserManager to find a user but then fail the password check
        _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(false);

        // Act
        var token = await _authService.LoginUserAsync(loginDto);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task LoginUserAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "nosuchuser@example.com", Password = "Password123!" };

        // Setup the mock UserManager to not find a user
        _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

        // Act
        var token = await _authService.LoginUserAsync(loginDto);

        // Assert
        Assert.Null(token);
    }
}