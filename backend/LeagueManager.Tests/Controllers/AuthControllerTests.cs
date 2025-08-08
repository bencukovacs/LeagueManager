using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace LeagueManager.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_WhenRegistrationIsSuccessful_ReturnsOkResult()
    {
        // Arrange
        var registerDto = new RegisterDto { FirstName = "Test", LastName = "User", Email = "test@test.com", Password = "Password123!" };
        _mockAuthService
            .Setup(s => s.RegisterUserAsync(registerDto))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_WhenRegistrationFails_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var registerDto = new RegisterDto { FirstName = "Test", LastName = "User", Email = "test@test.com", Password = "Password123!" };
        var errors = new List<IdentityError> { new() { Description = "Password is too weak." } };
        _mockAuthService
            .Setup(s => s.RegisterUserAsync(registerDto))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, badRequestResult.Value);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsOkResultWithToken()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@test.com", Password = "Password123!" };
        var fakeToken = "this_is_a_fake_jwt_token";

        // Mock a successful login that returns a token string
        _mockAuthService
            .Setup(s => s.LoginUserAsync(loginDto))
            .ReturnsAsync(fakeToken);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        // Check the anonymous object to ensure it contains the token
        var value = okResult.Value;
        Assert.NotNull(value);
        var tokenProperty = value.GetType().GetProperty("Token");
        Assert.NotNull(tokenProperty);
        Assert.Equal(fakeToken, tokenProperty.GetValue(value, null));
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@test.com", Password = "WrongPassword!" };

        // Mock a failed login that returns null
        _mockAuthService
            .Setup(s => s.LoginUserAsync(loginDto))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}