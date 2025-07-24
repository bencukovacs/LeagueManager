using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await _playerService.GetAllPlayersAsync();
        return Ok(players);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var player = await _playerService.GetPlayerByIdAsync(id);
        if (player == null)
        {
            return NotFound();
        }
        return Ok(player);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlayer([FromBody] PlayerDto playerDto)
    {
        try
        {
            var newPlayer = await _playerService.CreatePlayerAsync(playerDto);
            var createdPlayer = await _playerService.GetPlayerByIdAsync(newPlayer.Id);
            return CreatedAtAction(nameof(GetPlayer), new { id = createdPlayer!.Id }, createdPlayer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var success = await _playerService.DeletePlayerAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}