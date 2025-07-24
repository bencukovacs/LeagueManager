using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly LeagueDbContext _context;

    public LocationsController(LeagueDbContext context)
    {
        _context = context;
    }

    // GET: api/locations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        return await _context.Locations.ToListAsync();
    }

    // GET: api/locations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location == null)
        {
            return NotFound();
        }

        return location;
    }

    // POST: api/locations
    [HttpPost]
    public async Task<ActionResult<Location>> CreateLocation([FromBody] LocationDto locationDto)
    {
        var location = new Location
        {
            Name = locationDto.Name,
            Address = locationDto.Address,
            PitchNumber = locationDto.PitchNumber
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
    }

    // PUT: api/locations/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationDto locationDto)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location == null)
        {
            return NotFound();
        }

        location.Name = locationDto.Name;
        location.Address = locationDto.Address;
        location.PitchNumber = locationDto.PitchNumber;

        _context.Entry(location).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Locations.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/locations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return NotFound();
        }

        var isLocationInUse = await _context.Fixtures.AnyAsync(f => f.LocationId == id);
        if (isLocationInUse)
        {
            return BadRequest("Cannot delete location as it is currently assigned to one or more fixtures.");
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}