using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class CreateTeamDto
{
    public required string Name { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}

public class TeamResponseDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public required string Status { get; set; }
}
