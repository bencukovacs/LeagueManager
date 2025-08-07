using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IAuthorizationService _authorizationService;

    public PlayersController(IPlayerService playerService, IAuthorizationService authorizationService)
    {
        _playerService = playerService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await _playerService.GetAllPlayersAsync();
        return Ok(players);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
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
    [Authorize]
    public async Task<IActionResult> CreatePlayer([FromBody] PlayerDto playerDto)
    {
        try
        {
            var newPlayer = await _playerService.CreatePlayerAsync(playerDto);
            return CreatedAtAction(nameof(GetPlayer), new { id = newPlayer.Id }, newPlayer);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        try
        {
            await _playerService.DeletePlayerAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePlayer(int id, [FromBody] PlayerDto playerDto)
    {
        var playerDomainModel = await _playerService.GetDomainPlayerByIdAsync(id);
        if (playerDomainModel == null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, playerDomainModel, "CanUpdatePlayer");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var updatedPlayerDto = await _playerService.UpdatePlayerAsync(id, playerDto);
        return Ok(updatedPlayerDto);
    }
}