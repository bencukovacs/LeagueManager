using Microsoft.AspNetCore.Identity;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface IAuthService
{
    Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
    Task<bool> LoginUserAsync(LoginDto loginDto);
}