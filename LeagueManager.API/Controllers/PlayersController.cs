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
    [Authorize] // User must be logged in to create a player
    public async Task<IActionResult> CreatePlayer([FromBody] PlayerDto playerDto)
    {
        // Check if the user is authorized to manage the team they're adding a player to.
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, playerDto.TeamId, "CanManageTeam");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        try
        {
            var newPlayer = await _playerService.CreatePlayerAsync(playerDto);
            return CreatedAtAction(nameof(GetPlayer), new { id = newPlayer.Id }, newPlayer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePlayer(int id, [FromBody] PlayerDto playerDto)
    {
        var player = await _playerService.GetPlayerByIdAsync(id);
        if (player == null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, player.TeamId, "CanManageTeam");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var updatedPlayer = await _playerService.UpdatePlayerAsync(id, playerDto);
        return Ok(updatedPlayer);
    }

    [HttpDelete("{id}")]
    [Authorize] // User must be logged in to delete a player
    public async Task<IActionResult> DeletePlayer(int id)
    {
        // First, find the player to get their teamId
        var player = await _playerService.GetPlayerByIdAsync(id);
        if (player == null)
        {
            return NotFound();
        }

        // Now, check if the user is authorized to manage that player's team.
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, player.TeamId, "CanManageTeam");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var success = await _playerService.DeletePlayerAsync(id);
        if (!success)
        {
            // This case should be rare if the first check passed
            return NotFound();
        }
        return NoContent();
    }
}