using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class LeagueConfigurationControllerTests
{
    private readonly Mock<ILeagueConfigurationService> _mockConfigService;
    private readonly LeagueConfigurationController _controller;

    public LeagueConfigurationControllerTests()
    {
        _mockConfigService = new Mock<ILeagueConfigurationService>();
        _controller = new LeagueConfigurationController(_mockConfigService.Object);
    }

    [Fact]
    public async Task GetConfiguration_ReturnsOkResult_WithConfiguration()
    {
        // Arrange
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 5 };
        _mockConfigService.Setup(s => s.GetConfigurationAsync()).ReturnsAsync(configDto);

        // Act
        var result = await _controller.GetConfiguration();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConfig = Assert.IsType<LeagueConfigurationDto>(okResult.Value);
        Assert.Equal(5, returnedConfig.MinPlayersPerTeam);
    }

    [Fact]
    public async Task UpdateConfiguration_WithValidDto_ReturnsOkResultWithUpdatedConfig()
    {
        // Arrange
        var configDto = new LeagueConfigurationDto { MinPlayersPerTeam = 7 };
        _mockConfigService.Setup(s => s.UpdateConfigurationAsync(configDto)).ReturnsAsync(configDto);

        // Act
        var result = await _controller.UpdateConfiguration(configDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConfig = Assert.IsType<LeagueConfigurationDto>(okResult.Value);
        Assert.Equal(7, returnedConfig.MinPlayersPerTeam);
    }
}