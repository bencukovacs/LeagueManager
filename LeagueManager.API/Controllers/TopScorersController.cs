using LeagueManager.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopScorersController : ControllerBase
{
    private readonly TopScorersService _topScorersService;

    public TopScorersController(TopScorersService topScorersService)
    {
        _topScorersService = topScorersService;
    }

  // GET: api/topscorers
  [HttpGet]
    public async Task<IActionResult> GetTopScorers()
    {
        var topScorers = await _topScorersService.GetTopScorersAsync();
        return Ok(topScorers);
    }
}