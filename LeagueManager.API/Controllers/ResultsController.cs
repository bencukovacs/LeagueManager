using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.API.Controllers;

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
    [HttpPost("fixtures/{fixtureId}/results")]
    public async Task<IActionResult> SubmitResult(int fixtureId, [FromBody] SubmitResultDto resultDto)
    {
        var fixture = await _context.Fixtures.FindAsync(fixtureId);
        if (fixture == null)
        {
            return NotFound("Fixture not found.");
        }

        // Validation Check #1: Ensure a result doesn't already exist
        if (await _context.Results.AnyAsync(r => r.FixtureId == fixtureId))
        {
            return BadRequest("A result for this fixture has already been submitted.");
        }

        // Validation Check #2: Ensure the score matches the number of goalscorers
        var totalGoals = resultDto.Goalscorers.Count;
        if (totalGoals != resultDto.HomeScore + resultDto.AwayScore)
        {
            return BadRequest("The number of goalscorers does not match the total score.");
        }

        // Validation Check #3: Ensure all goalscorer IDs are valid players for the teams in the fixture
        var playerIds = resultDto.Goalscorers.Select(g => g.PlayerId).ToList();
        if (playerIds.Any()) // Only check if there are any goalscorers
        {
            var validPlayersCount = await _context.Players
                .CountAsync(p => playerIds.Contains(p.Id) && (p.TeamId == fixture.HomeTeamId || p.TeamId == fixture.AwayTeamId));

            if (validPlayersCount != playerIds.Count)
            {
                return BadRequest("One or more goalscorer IDs are invalid or do not belong to the competing teams.");
            }
        }

        // This part remains the same, but now it's protected by the validation above
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
            
            // Return the full result object on success
            return Ok(result);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An internal error occurred while submitting the result.");
        }
    }
    
    // PATCH: api/results/5/status
    [HttpPatch("results/{resultId}/status")]
    public async Task<IActionResult> UpdateResultStatus(int resultId, [FromBody] UpdateResultStatusDto statusDto)
    {
        var result = await _context.Results.FindAsync(resultId);

        if (result == null)
        {
            return NotFound("Result not found.");
        }

        // Update the status and save the change
        result.Status = statusDto.Status;
        await _context.SaveChangesAsync();

        return NoContent(); // Indicates success with no content to return
    }
}