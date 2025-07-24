using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly LeagueDbContext _context;

    public ResultsController(LeagueDbContext context)
    {
        _context = context;
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

        result.Status = statusDto.Status;
        await _context.SaveChangesAsync();

        return NoContent(); 
    }
}