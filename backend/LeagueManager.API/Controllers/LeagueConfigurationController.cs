using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // This entire controller is for Admins only
public class LeagueConfigurationController : ControllerBase
{
    private readonly ILeagueConfigurationService _configService;

    public LeagueConfigurationController(ILeagueConfigurationService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public async Task<IActionResult> GetConfiguration()
    {
        var config = await _configService.GetConfigurationAsync();
        return Ok(config);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateConfiguration([FromBody] LeagueConfigurationDto configDto)
    {
        var updatedConfig = await _configService.UpdateConfigurationAsync(configDto);
        return Ok(updatedConfig);
    }
}