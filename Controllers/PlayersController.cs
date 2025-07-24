using LeagueManager.Data;
using LeagueManager.Dtos;
using LeagueManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
  private readonly LeagueDbContext _context;

  public PlayersController(LeagueDbContext context)
  {
    _context = context;
  }

  // GET: api/players
  [HttpGet]
  public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
  {
    var players = await _context.Players.Include(p => p.Team).ToListAsync();
    return Ok(players);
  }

  // GET api/players/5
  [HttpGet("{id}")]
  public async Task<ActionResult<Player>> GetPlayer(int id)
  {
    var player = await _context.Players
          .Include(p => p.Team)
          .FirstOrDefaultAsync(p => p.Id == id);

    if (player == null)
    {
      return NotFound();
    }

    return Ok(player);
  }

  // POST: api/players
  [HttpPost]
  public async Task<ActionResult<Player>> CreatePlayer([FromBody] PlayerDto playerDto)
  {
    var team = await _context.Teams.FindAsync(playerDto.TeamId);
    if (team == null)
    {
      return BadRequest("Invalid Team ID.");
    }

    var player = new Player
    {
      Name = playerDto.Name,
      TeamId = playerDto.TeamId
    };

    _context.Players.Add(player);
    await _context.SaveChangesAsync();

    await _context.Entry(player).Reference(p => p.Team).LoadAsync();

    return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
  }

  // PUT api/players/5
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdatePlayer(int id, [FromBody] PlayerDto playerDto)
  {
    var player = await _context.Players.FindAsync(id);
    if (player == null)
    {
      return NotFound();
    }

    var teamExists = await _context.Teams.AnyAsync(t => t.Id == playerDto.TeamId);
    if (!teamExists)
    {
      return NotFound();
    }

    player.Name = playerDto.Name;
    player.TeamId = playerDto.TeamId;

    _context.Entry(player).State = EntityState.Modified;

    try
    {
      await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!_context.Players.Any(e => e.Id == id))
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

  // DELETE: api/players/5
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeletePlayer(int id)
  {
    var player = await _context.Players.FindAsync(id);
    if (player == null)
    {
      return NotFound();
    }

    _context.Players.Remove(player);
    await _context.SaveChangesAsync();

    return NoContent();
  }
}