namespace LeagueManager.API.Dtos;

public class TopScorerDto
{
    public required string PlayerName { get; set; }
    public required string TeamName { get; set; }
    public int Goals { get; set; }
}