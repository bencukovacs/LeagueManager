using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/me")]
[Authorize] // All endpoints in this controller require a user to be logged in
public class MeController : ControllerBase
{
    private readonly IRosterRequestService _rosterRequestService;

    public MeController(IRosterRequestService rosterRequestService)
    {
        _rosterRequestService = rosterRequestService;
    }

    [HttpGet("roster-requests")]
    public async Task<IActionResult> GetMyPendingRequests()
    {
        var requests = await _rosterRequestService.GetMyPendingRequestsAsync();
        return Ok(requests);
    }
}