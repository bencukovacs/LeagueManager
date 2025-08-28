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
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, playerDto.TeamId, "CanEditRoster");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }
        try
        {
            var newPlayer = await _playerService.CreatePlayerAsync(playerDto);
            return CreatedAtAction(nameof(GetPlayer), new { id = newPlayer.Id }, newPlayer);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException ex)
        {
            // Return a 403 Forbidden status with a custom error object
            return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/remove-from-roster")]
    [Authorize]
    public async Task<IActionResult> RemovePlayerFromRoster(int id)
    {
        var player = await _playerService.GetPlayerByIdAsync(id);
        if (player == null)
        {
            return NotFound();
        }

        // Authorization: Can the current user manage this player's team?
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, player.TeamId, "CanEditRoster");
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        await _playerService.RemovePlayerFromRosterAsync(id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePlayerPermanently(int id)
    {
        try
        {
            await _playerService.DeletePlayerPermanentlyAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) 
        { 
            return NotFound(ex.Message); 
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
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

    [HttpGet("unassigned")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUnassignedPlayers()
    {
        var players = await _playerService.GetUnassignedPlayersAsync();
        return Ok(players);
    }
    
    [HttpPatch("{playerId}/assign/{teamId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignPlayerToTeam(int playerId, int teamId)
    {
        var updatedPlayer = await _playerService.AssignPlayerToTeamAsync(playerId, teamId);
        if (updatedPlayer == null)
        {
            return NotFound("Player or Team not found.");
        }
        return Ok(updatedPlayer);
    }
    
}