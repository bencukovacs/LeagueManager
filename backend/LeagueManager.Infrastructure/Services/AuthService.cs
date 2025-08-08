using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeagueManager.Application.Settings;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly LeagueDbContext _context;

    public AuthService(UserManager<User> userManager, IOptions<JwtSettings> jwtSettings, LeagueDbContext context)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _context = context;
    }

    public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
    {
        var user = new User { UserName = registerDto.Email, Email = registerDto.Email };
        
        // This is a special case where we need two saves, but UserManager handles it.
        // First, create the user.
        var result = await _userManager.CreateAsync(user, registerDto.Password);
        
        if (!result.Succeeded)
        {
            return result; // Return immediately if user creation fails.
        }

        // If user creation succeeds, then add the role and the player.
        await _userManager.AddToRoleAsync(user, "RegisteredUser");

        var player = new Player
        {
            Name = $"{registerDto.FirstName} {registerDto.LastName}",
            UserId = user.Id,
        };
        _context.Players.Add(player);
        
        // This single SaveChangesAsync call will save the new Player and the user's new Role.
        await _context.SaveChangesAsync();
        
        return result;
    }

    public async Task<string?> LoginUserAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            // Pass the user object to the token generator
            return await GenerateJwtToken(user);
        }

        return null;
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };

        // Get the user's roles from the database
        var roles = await _userManager.GetRolesAsync(user);
        
        // Add each role as a separate "role" claim to the token
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

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