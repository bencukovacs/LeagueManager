using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLocations()
    {
        return Ok(await _locationService.GetAllLocationsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLocation(int id)
    {
        var location = await _locationService.GetLocationByIdAsync(id);
        if (location == null)
        {
            return NotFound();
        }
        return Ok(location);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLocation([FromBody] LocationDto locationDto)
    {
        var newLocation = await _locationService.CreateLocationAsync(locationDto);
        return CreatedAtAction(nameof(GetLocation), new { id = newLocation.Id }, newLocation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationDto locationDto)
    {
        var location = await _locationService.UpdateLocationAsync(id, locationDto);
        if (location == null)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        try
        {
            var success = await _locationService.DeleteLocationAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}