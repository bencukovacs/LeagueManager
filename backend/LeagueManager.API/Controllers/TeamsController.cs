using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly IPlayerService _playerService;
    private readonly IAuthorizationService _authorizationService;

    public TeamsController(ITeamService teamService, IPlayerService playerService, IAuthorizationService authorizationService)
    {
        _teamService = teamService;
        _playerService = playerService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeams()
    {
        return Ok(await _teamService.GetAllTeamsAsync());
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAllTeamsForAdmin()
    {
        return Ok(await _teamService.GetAllTeamsForAdminAsync());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeam(int id)
    {
        var team = await _teamService.GetTeamByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }
        return Ok(team);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto createTeamDto)
    {
        try
        {
            var newTeam = await _teamService.CreateTeamAsync(createTeamDto);
            return CreatedAtAction(nameof(GetTeam), new { id = newTeam.Id }, newTeam);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTeamAsAdmin([FromBody] CreateTeamDto createTeamDto)
    {
        var newTeam = await _teamService.CreateTeamAsAdminAsync(createTeamDto);
        return CreatedAtAction(nameof(GetTeam), new { id = newTeam.Id }, newTeam);
    }

    [HttpPatch("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveTeam(int id)
    {
        try
        {
            var team = await _teamService.ApproveTeamAsync(id);
            if (team == null)
            {
                return NotFound("Team not found.");
            }
            return Ok(team);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] CreateTeamDto updateTeamDto)
    {
        // Perform the resource-based authorization check
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, id, "CanManageTeam");
        if (!authorizationResult.Succeeded)
        {
            // If the policy fails, return a 403 Forbidden or 404 Not Found
            return Forbid();
        }

        // If authorization succeeds, proceed with the update
        var team = await _teamService.UpdateTeamAsync(id, updateTeamDto);
        if (team == null)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var success = await _teamService.DeleteTeamAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingTeams()
    {
        var pendingTeams = await _teamService.GetPendingTeamsAsync();
        return Ok(pendingTeams);
    }

    [HttpGet("{teamId}/players")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeamRoster(int teamId)
    {
        var players = await _playerService.GetPlayersForTeamAsync(teamId);
        return Ok(players);
    }
}