using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly IResultService _resultService;

    public ResultsController(IResultService resultService)
    {
        _resultService = resultService;
    }

    [HttpPatch("{resultId}/status")]
    public async Task<IActionResult> UpdateResultStatus(int resultId, [FromBody] UpdateResultStatusDto statusDto)
    {
        var result = await _resultService.UpdateResultStatusAsync(resultId, statusDto);

        if (result == null)
        {
            return NotFound("Result not found.");
        }

        return NoContent();
    }
}