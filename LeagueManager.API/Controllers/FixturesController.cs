using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixturesController : ControllerBase
{
    private readonly IFixtureService _fixtureService;
    private readonly IAuthorizationService _authorizationService;

    public FixturesController(IFixtureService fixtureService, IAuthorizationService authorizationService)
    {
        _fixtureService = fixtureService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFixtures()
    {
        var fixtures = await _fixtureService.GetAllFixturesAsync();
        return Ok(fixtures);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFixture(int id)
    {
        var fixture = await _fixtureService.GetFixtureByIdAsync(id);
        if (fixture == null)
        {
            return NotFound();
        }
        return Ok(fixture);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFixture([FromBody] CreateFixtureDto createFixtureDto)
    {
        try
        {
            var newFixture = await _fixtureService.CreateFixtureAsync(createFixtureDto);
            return CreatedAtAction(nameof(GetFixture), new { id = newFixture.Id }, newFixture);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{fixtureId}/results")]
    [Authorize]
    public async Task<IActionResult> SubmitResult(int fixtureId, [FromBody] SubmitResultDto resultDto)
    {
        var fixture = await _fixtureService.GetFixtureByIdAsync(fixtureId);
        if (fixture == null)
        {
            return NotFound("Fixture not found.");
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, fixture, "CanSubmitResult");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        try
        {
            var result = await _fixtureService.SubmitResultAsync(fixtureId, resultDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateFixture(int id, [FromBody] UpdateFixtureDto updateFixtureDto)
    {
        try
        {
            var fixture = await _fixtureService.UpdateFixtureAsync(id, updateFixtureDto);
            if (fixture == null)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFixture(int id)
    {
        var success = await _fixtureService.DeleteFixtureAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}