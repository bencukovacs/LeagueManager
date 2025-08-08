namespace LeagueManager.Application.Dtos;

public class RosterRequestResponseDto
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string TeamName { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
}