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
        var playerDtos = await _playerService.GetAllPlayersAsync();
        return Ok(playerDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var playerDto = await _playerService.GetPlayerByIdAsync(id);
        if (playerDto == null)
        {
            return NotFound();
        }
        return Ok(playerDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlayer([FromBody] PlayerDto playerDto)
    {
        try
        {
            var createdPlayerDto = await _playerService.CreatePlayerAsync(playerDto);
            return CreatedAtAction(nameof(GetPlayer), new { id = createdPlayerDto.Id }, createdPlayerDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(int id, [FromBody] PlayerDto playerDto)
    {
        var player = await _playerService.UpdatePlayerAsync(id, playerDto);
        if (player == null)
        {
            return NotFound();
        }
        return NoContent();

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
