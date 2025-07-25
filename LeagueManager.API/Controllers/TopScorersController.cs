using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopScorersController : ControllerBase
{
    private readonly ITopScorersService _topScorersService;

    public TopScorersController(ITopScorersService topScorersService)
    {
        _topScorersService = topScorersService;
    }

    // GET: api/topscorers
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTopScorers()
    {
        var topScorers = await _topScorersService.GetTopScorersAsync();
        return Ok(topScorers);
    }
}