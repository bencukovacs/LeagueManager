using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/roster-requests")]
[Authorize]
public class RosterRequestsController : ControllerBase
{
  private readonly IRosterRequestService _rosterRequestService;

  public RosterRequestsController(IRosterRequestService rosterRequestService)
  {
    _rosterRequestService = rosterRequestService;
  }

  [HttpPost("join/{teamId}")]
  public async Task<IActionResult> CreateJoinRequest(int teamId)
  {
    try
    {
      var request = await _rosterRequestService.CreateJoinRequestAsync(teamId);
      return Ok(request);
    }
    catch (ArgumentException ex) { return NotFound(ex.Message); }
    catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
  }

  [HttpGet("my-team/pending")]
  public async Task<IActionResult> GetPendingJoinRequestsForMyTeam()
  {
    var requests = await _rosterRequestService.GetPendingJoinRequestsForMyTeamAsync();
    return Ok(requests);
  }
    
    [HttpPatch("{requestId}/approve")]
    public async Task<IActionResult> ApproveJoinRequest(int requestId)
    {
        try
        {
            await _rosterRequestService.ApproveJoinRequestAsync(requestId);
            return Ok(new { Message = "Join request approved successfully." });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPatch("{requestId}/reject")]
    public async Task<IActionResult> RejectJoinRequest(int requestId)
    {
        try
        {
            await _rosterRequestService.RejectJoinRequestAsync(requestId);
            return Ok(new { Message = "Join request rejected successfully." });
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}