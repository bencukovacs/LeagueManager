using Moq;
using LeagueManager.API.Controllers;
using LeagueManager.Application.Services;
using LeagueManager.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Tests.Controllers;

public class ResultsControllerTests
{
    private readonly Mock<IResultService> _mockResultService;
    private readonly ResultsController _controller;

    public ResultsControllerTests()
    {
        _mockResultService = new Mock<IResultService>();
        _controller = new ResultsController(_mockResultService.Object);
    }

    [Fact]
    public async Task UpdateResultStatus_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var dto = new UpdateResultStatusDto { Status = LeagueManager.Domain.Models.ResultStatus.Approved };
        var responseDto = new ResultResponseDto { Id = 1, FixtureId = 1, Status = "Approved", HomeScore = 1, AwayScore = 0 };

        // The service now returns the DTO on success
        _mockResultService
            .Setup(service => service.UpdateResultStatusAsync(1, dto))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.UpdateResultStatus(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateResultStatus_WhenResultNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateResultStatusDto { Status = LeagueManager.Domain.Models.ResultStatus.Approved };

        // The service now returns null when the result is not found
        _mockResultService
            .Setup(service => service.UpdateResultStatusAsync(99, dto))
            .ReturnsAsync((ResultResponseDto?)null);

        // Act
        var result = await _controller.UpdateResultStatus(99, dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Result not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateResultStatus_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var dto = new UpdateResultStatusDto { Status = LeagueManager.Domain.Models.ResultStatus.Approved };
        var responseDto = new ResultResponseDto { Id = 5, FixtureId = 1, Status = "Approved", HomeScore = 1, AwayScore = 0 };

        _mockResultService
            .Setup(s => s.UpdateResultStatusAsync(It.IsAny<int>(), It.IsAny<UpdateResultStatusDto>()))
            .ReturnsAsync(responseDto);

        // Act
        await _controller.UpdateResultStatus(5, dto);

        // Assert
        // Verify that the service method was called exactly once with the correct parameters
        _mockResultService.Verify(s => s.UpdateResultStatusAsync(5, dto), Times.Once);
    }
}