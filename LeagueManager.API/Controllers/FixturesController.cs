using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixturesController : ControllerBase
{
    private readonly IFixtureService _fixtureService;

    public FixturesController(IFixtureService fixtureService)
    {
        _fixtureService = fixtureService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFixtures()
    {
        return Ok(await _fixtureService.GetAllFixturesAsync());
    }
    
    [HttpGet("{id}")]
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
    public async Task<IActionResult> SubmitResult(int fixtureId, [FromBody] SubmitResultDto resultDto)
    {
        try
        {
            var result = await _fixtureService.SubmitResultAsync(fixtureId, resultDto);
            return Ok(result);
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