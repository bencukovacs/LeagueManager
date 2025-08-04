namespace LeagueManager.Application.Dtos;

public class MyTeamResponseDto
{
    public required TeamResponseDto Team { get; set; }
    public required string UserRole { get; set; }
}