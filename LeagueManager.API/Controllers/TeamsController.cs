using LeagueManager.Data;
using LeagueManager.Dtos;
using LeagueManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly LeagueDbContext _context;

    public TeamsController(LeagueDbContext context)
    {
        _context = context;
    }

    // GET: api/teams
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
    {
        var teams = await _context.Teams.ToListAsync();
        return Ok(teams);
    }

    // GET: api/teams/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if(team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    // POST: api/teams
    [HttpPost]
    public async Task<ActionResult<Team>> CreateTeam([FromBody] CreateTeamDto createTeamDto)
    {
        var team = new Team
        {
            Name = createTeamDto.Name,
            PrimaryColor = createTeamDto.PrimaryColor,
            SecondaryColor = createTeamDto.SecondaryColor
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetTeam), new { id= team.Id }, team);
    }

    // PUT: api/teams/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] CreateTeamDto updateTeamDto)
    {
        var team = await _context.Teams.FindAsync(id);
        if(team == null)
        {
            return NotFound();
        }

        team.Name = updateTeamDto.Name;
        team.PrimaryColor = updateTeamDto.PrimaryColor;
        team.SecondaryColor = updateTeamDto.SecondaryColor;

        _context.Entry(team).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException)
        {
            if(!_context.Teams.Any(e => e.Id == id))
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

    // DELETE: api/teams/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if(team == null)
        {
            return NotFound();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}