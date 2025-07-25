using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var result = await _authService.RegisterUserAsync(registerDto);

        if (result.Succeeded)
        {
            return Ok(new { Message = "User registered successfully" });
        }

        // If registration fails, return the errors
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var token = await _authService.LoginUserAsync(loginDto);

        if (token != null)
        {
            // Return the token in the response
            return Ok(new { Token = token });
        }

        return Unauthorized(new { Message = "Invalid credentials" });
    }
}