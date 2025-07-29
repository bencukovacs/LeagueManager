using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class RegisterDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}

public class LoginDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}