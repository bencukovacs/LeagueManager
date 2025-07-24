using LeagueManager.Data;
using LeagueManager.Dtos;
using LeagueManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Controllers;

[ApiController]
[Route("api/fixtures/{fixtureId}/results")]
public class ResultsController : ControllerBase
{
    private readonly LeagueDbContext _context;

    public ResultsController(LeagueDbContext context)
    {
        _context = context;
    }

    // POST: api/fixtures/5/results
    [HttpPost]
    public async Task<IActionResult> SubmitResult(int fixtureId, [FromBody] SubmitResultDto resultDto)
    {
        var fixture = await _context.Fixtures
            .Include(f => f.HomeTeam)
            .Include(f => f.AwayTeam)
            .FirstOrDefaultAsync(f => f.Id == fixtureId);

        if (fixture == null)
        {
            return NotFound("Fixture not found.");
        }

        if (fixture.Status == FixtureStatus.Completed)
        {
            return BadRequest("A result has already been submitted for this fixture.");
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
            
            foreach (var goalscorer in resultDto.Goalscorers)
            {
                var goal = new Goal
                {
                    PlayerId = goalscorer.PlayerId,
                    FixtureId = fixtureId
                };
                _context.Goals.Add(goal);
            }

            fixture.Status = FixtureStatus.Completed;
            _context.Entry(fixture).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok("Result submitted successfully and is pending approval.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while submitting the result.");
        }
    }
}