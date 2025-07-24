using LeagueManager.API.Controllers;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
    public async Task UpdateResultStatus_WithValidResultId_ReturnsNoContent()
    {
        // Arrange
        var dto = new UpdateResultStatusDto { Status = ResultStatus.Approved };
        _mockResultService
            .Setup(service => service.UpdateResultStatusAsync(1, dto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateResultStatus(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateResultStatus_WithInvalidResultId_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateResultStatusDto { Status = ResultStatus.Approved };
        _mockResultService
            .Setup(service => service.UpdateResultStatusAsync(99, dto))
            .ReturnsAsync(false);

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
        var dto = new UpdateResultStatusDto { Status = ResultStatus.Approved };

        _mockResultService
            .Setup(s => s.UpdateResultStatusAsync(It.IsAny<int>(), It.IsAny<UpdateResultStatusDto>()))
            .ReturnsAsync(true);

        // Act
        await _controller.UpdateResultStatus(5, dto);

        // Assert
        _mockResultService.Verify(s => s.UpdateResultStatusAsync(5, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateResultStatus_WithNullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UpdateResultStatus(1, null!);

        // Assert
        var badRequest = Assert.IsType<BadRequestResult>(result);
    }
}
