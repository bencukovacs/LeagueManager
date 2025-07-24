using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeagueTableController : ControllerBase
{
    private readonly ILeagueTableService _leagueTableService;

    public LeagueTableController(ILeagueTableService leagueTableService)
    {
        _leagueTableService = leagueTableService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var table = await _leagueTableService.GetLeagueTableAsync();
        return Ok(table);
    }
}