using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await _locationService.GetAllLocationsAsync();
        return Ok(locations);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateLocation([FromBody] LocationDto locationDto)
    {
        var newLocation = await _locationService.CreateLocationAsync(locationDto);
        return CreatedAtAction(nameof(GetLocation), new { id = newLocation.Id }, newLocation);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var success = await _locationService.DeleteLocationAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}