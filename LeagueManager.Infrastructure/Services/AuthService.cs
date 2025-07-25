using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options; // Add this using for IOptions
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeagueManager.Application.Settings; // Add this to access JwtSettings

namespace LeagueManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSettings _jwtSettings; // Use the strongly-typed settings class

    // Inject IOptions<JwtSettings> instead of IConfiguration
    public AuthService(UserManager<User> userManager, IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        // The .Value property gives us the actual settings object, which is guaranteed not to be null
        _jwtSettings = jwtSettings.Value; 
    }

    public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
    {
        var user = new User { UserName = registerDto.Email, Email = registerDto.Email };
        var result = await _userManager.CreateAsync(user, registerDto.Password);
        return result;
    }

    public async Task<string?> LoginUserAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return GenerateJwtToken(user);
        }

        return null;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id)
        };
        
        // FIX: Add a null check for user.Email before adding the claim
        if (user.Email != null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        // Now we get settings from our null-safe _jwtSettings field
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}