using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/my-team")]
[Authorize] // This entire controller requires a user to be logged in
public class MyTeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public MyTeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTeam()
    {
        var team = await _teamService.GetMyTeamAsync();
        if (team == null)
        {
            // This means the user is logged in but is not a leader of any team
            return NotFound("You do not currently have a team.");
        }
        return Ok(team);
    }
}