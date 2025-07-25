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

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeams()
    {
        return Ok(await _teamService.GetAllTeamsAsync());
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

    [HttpPatch("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveTeam(int id)
    {
        var team = await _teamService.ApproveTeamAsync(id);
        if (team == null)
        {
            return NotFound("Team not found.");
        }
        return Ok(team);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] CreateTeamDto updateTeamDto)
    {
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
}