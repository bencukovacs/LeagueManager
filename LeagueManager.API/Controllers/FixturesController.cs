using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixturesController : ControllerBase
{
    private readonly LeagueDbContext _context;

    public FixturesController(LeagueDbContext context)
    {
        _context = context;
    }

    // GET: api/fixtures
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fixture>>> GetFixtures()
    {
        var fixtures = await _context.Fixtures
            .Include(f => f.HomeTeam)
            .Include(f => f.AwayTeam)
            .Include(f => f.Location)
            .ToListAsync();
        return Ok(fixtures);
    }

    // GET: api/fixtures/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Fixture>> GetFixture(int id)
    {
        var fixture = await _context.Fixtures
            .Include(f => f.HomeTeam)
            .Include(f => f.AwayTeam)
            .Include(f => f.Location)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fixture == null)
        {
            return NotFound();
        }

        return Ok(fixture);
    }

    // POST: api/fixtures
    [HttpPost]
    public async Task<ActionResult<Fixture>> CreateFixture([FromBody] CreateFixtureDto createFixtureDto)
    {
        if (createFixtureDto.HomeTeamId == createFixtureDto.AwayTeamId)
        {
            return BadRequest("Home team and away team cannot be the same.");
        }

        var homeTeam = await _context.Teams.FindAsync(createFixtureDto.HomeTeamId);
        var awayTeam = await _context.Teams.FindAsync(createFixtureDto.AwayTeamId);

        if (homeTeam == null || awayTeam == null)
        {
            return BadRequest("One or both teams do not exist.");
        }

        if (createFixtureDto.LocationId.HasValue)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == createFixtureDto.LocationId.Value);
            if (!locationExists)
            {
                return BadRequest("The specified location does not exist.");
            }
        }

        var fixture = new Fixture
        {
            HomeTeamId = createFixtureDto.HomeTeamId,
            AwayTeamId = createFixtureDto.AwayTeamId,
            KickOffDateTime = createFixtureDto.KickOffDateTime,
            LocationId = createFixtureDto.LocationId,
            Status = FixtureStatus.Scheduled
        };

        _context.Fixtures.Add(fixture);
        await _context.SaveChangesAsync();

        await _context.Entry(fixture).Reference(f => f.HomeTeam).LoadAsync();
        await _context.Entry(fixture).Reference(f => f.AwayTeam).LoadAsync();
        if (fixture.LocationId.HasValue)
        {
            await _context.Entry(fixture).Reference(f => f.Location).LoadAsync();
        }

        return CreatedAtAction(nameof(GetFixture), new { id = fixture.Id }, fixture);
    }

    // PUT: api/fixtures/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFixture(int id, [FromBody] UpdateFixtureDto updateFixtureDto)
    {
        var fixture = await _context.Fixtures.FindAsync(id);

        if (fixture == null)
        {
            return NotFound();
        }
        
        fixture.KickOffDateTime = updateFixtureDto.KickOffDateTime;
        fixture.LocationId = updateFixtureDto.LocationId;

        if (updateFixtureDto.LocationId.HasValue)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == updateFixtureDto.LocationId.Value);
            if (!locationExists)
            {
                return BadRequest("The specified location does not exist.");
            }
        }

        _context.Entry(fixture).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/fixtures/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFixture(int id)
    {
        var fixture = await _context.Fixtures.FindAsync(id);
        if (fixture == null)
        {
            return NotFound();
        }

        _context.Fixtures.Remove(fixture);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
