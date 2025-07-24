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

    // POST: api/fixtures/5/results
    [HttpPost("{fixtureId}/results")]
    public async Task<IActionResult> SubmitResult(int fixtureId, [FromBody] SubmitResultDto resultDto)
    {
        var fixture = await _context.Fixtures.FindAsync(fixtureId);
        if (fixture == null)
        {
            return NotFound("Fixture not found.");
        }

        if (await _context.Results.AnyAsync(r => r.FixtureId == fixtureId))
        {
            return BadRequest("A result for this fixture has already been submitted.");
        }

        var totalGoals = resultDto.Goalscorers.Count;
        if (totalGoals != resultDto.HomeScore + resultDto.AwayScore)
        {
            return BadRequest("The number of goalscorers does not match the total score.");
        }

        var playerIds = resultDto.Goalscorers.Select(g => g.PlayerId).ToList();
        if (playerIds.Any())
        {
            var validPlayersCount = await _context.Players
                .CountAsync(p => playerIds.Contains(p.Id) && (p.TeamId == fixture.HomeTeamId || p.TeamId == fixture.AwayTeamId));

            if (validPlayersCount != playerIds.Count)
            {
                return BadRequest("One or more goalscorer IDs are invalid or do not belong to the competing teams.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = new Result
            {
                FixtureId = fixtureId,
                HomeScore = resultDto.HomeScore,
                AwayScore = resultDto.AwayScore,
                Status = ResultStatus.PendingApproval
            };
            _context.Results.Add(result);

            fixture.Status = FixtureStatus.Completed;
            _context.Fixtures.Update(fixture);

            foreach (var goalscorer in resultDto.Goalscorers)
            {
                var goal = new Goal
                {
                    PlayerId = goalscorer.PlayerId,
                    FixtureId = fixtureId
                };
                _context.Goals.Add(goal);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return Ok(result);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An internal error occurred while submitting the result.");
        }
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
